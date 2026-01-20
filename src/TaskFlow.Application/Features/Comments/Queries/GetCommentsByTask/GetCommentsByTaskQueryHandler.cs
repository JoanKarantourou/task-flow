using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Comments.Queries.GetCommentsByTask;

/// <summary>
/// Handler for GetCommentsByTaskQuery.
/// Gets all comments for a specific task.
/// </summary>
public class GetCommentsByTaskQueryHandler : IRequestHandler<GetCommentsByTaskQuery, List<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGenericRepository<Comment> _commentRepository;
    private readonly IGenericRepository<User> _userRepository;

    public GetCommentsByTaskQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IGenericRepository<Comment> commentRepository,
        IGenericRepository<User> userRepository)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
        _userRepository = userRepository;
    }

    public async Task<List<CommentDto>> Handle(
        GetCommentsByTaskQuery request,
        CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to view comments");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Verify the task exists and user has access
        var task = await _unitOfWork.Tasks.GetTaskWithDetailsAsync(request.TaskId, cancellationToken);

        if (task == null)
        {
            throw new ArgumentException($"Task with ID {request.TaskId} not found");
        }

        // Verify user has access to the project
        var project = await _unitOfWork.Projects.GetByIdAsync(task.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new ArgumentException($"Project not found for task {request.TaskId}");
        }

        bool hasAccess = project.OwnerId == currentUserId || task.AssigneeId == currentUserId;

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("You don't have permission to view comments on this task");
        }

        // Get comments for the task
        var comments = await _commentRepository.FindAsync(
            c => c.TaskId == request.TaskId,
            cancellationToken);

        // Get all unique author IDs
        var authorIds = comments.Select(c => c.AuthorId).Distinct().ToList();

        // Fetch all authors in one query
        var authors = await _userRepository.FindAsync(
            u => authorIds.Contains(u.Id),
            cancellationToken);

        var authorDict = authors.ToDictionary(a => a.Id);

        // Order by creation date and map to DTOs
        var commentDtos = comments
            .OrderBy(c => c.CreatedAt)
            .Select(c =>
            {
                var author = authorDict.GetValueOrDefault(c.AuthorId);
                return new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    TaskId = c.TaskId,
                    AuthorId = c.AuthorId,
                    AuthorName = author?.FullName ?? "Unknown",
                    AuthorEmail = author?.Email ?? "",
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                };
            })
            .ToList();

        return commentDtos;
    }
}
