using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Features.Dashboard.Queries.GetDashboardStats;

/// <summary>
/// Handler for GetDashboardStatsQuery.
/// Aggregates statistics from projects and tasks the user has access to.
/// </summary>
public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetDashboardStatsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DashboardStatsDto> Handle(
        GetDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Get all projects the user has access to
        var projects = await _unitOfWork.Projects.GetProjectsByMemberAsync(
            currentUserId,
            cancellationToken);

        // Get all tasks from user's projects using the existing paged method
        // Use a large page size to get all tasks for aggregation
        var (tasks, _) = await _unitOfWork.Tasks.GetTasksPagedAsync(
            new TaskFilterParameters { PageSize = 10000 },
            currentUserId,
            cancellationToken);

        var now = DateTime.UtcNow;

        // Calculate stats
        var stats = new DashboardStatsDto
        {
            TotalProjects = projects.Count,
            ActiveProjects = projects.Count(p => p.Status == ProjectStatus.Active),
            TotalTasks = tasks.Count,
            PendingTasks = tasks.Count(t => t.Status == TaskStatus.Todo || t.Status == TaskStatus.InProgress),
            CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Done),
            OverdueTasks = tasks.Count(t =>
                t.DueDate.HasValue &&
                t.DueDate.Value < now &&
                t.Status != TaskStatus.Done &&
                t.Status != TaskStatus.Cancelled),
            TasksByStatus = new Dictionary<string, int>
            {
                ["Todo"] = tasks.Count(t => t.Status == TaskStatus.Todo),
                ["InProgress"] = tasks.Count(t => t.Status == TaskStatus.InProgress),
                ["InReview"] = tasks.Count(t => t.Status == TaskStatus.InReview),
                ["Done"] = tasks.Count(t => t.Status == TaskStatus.Done),
                ["Cancelled"] = tasks.Count(t => t.Status == TaskStatus.Cancelled)
            },
            TasksByPriority = new Dictionary<string, int>
            {
                ["Low"] = tasks.Count(t => t.Priority == TaskPriority.Low),
                ["Medium"] = tasks.Count(t => t.Priority == TaskPriority.Medium),
                ["High"] = tasks.Count(t => t.Priority == TaskPriority.High),
                ["Critical"] = tasks.Count(t => t.Priority == TaskPriority.Critical)
            }
        };

        return stats;
    }
}
