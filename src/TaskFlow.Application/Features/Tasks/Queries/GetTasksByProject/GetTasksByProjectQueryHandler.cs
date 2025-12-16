using AutoMapper;
using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Features.Tasks.Queries.GetTasksByProject;

/// <summary>
/// Handler for GetTasksByProjectQuery.
/// Returns all tasks for a project if user has access.
/// </summary>
public class GetTasksByProjectQueryHandler : IRequestHandler<GetTasksByProjectQuery, List<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetTasksByProjectQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<List<TaskDto>> Handle(
        GetTasksByProjectQuery request,
        CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Check if user has access to the project
        var hasAccess = await _unitOfWork.Projects.UserHasAccessToProjectAsync(
            request.ProjectId,
            currentUserId,
            cancellationToken);

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException(
                "You don't have permission to view tasks in this project");
        }

        // Get all tasks for the project
        var tasks = await _unitOfWork.Tasks.GetTasksByProjectAsync(
            request.ProjectId,
            cancellationToken);

        // Convert to DTOs
        return _mapper.Map<List<TaskDto>>(tasks);
    }
}