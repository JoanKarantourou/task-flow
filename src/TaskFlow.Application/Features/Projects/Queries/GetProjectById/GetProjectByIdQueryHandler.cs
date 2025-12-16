using AutoMapper;
using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Features.Projects.Queries.GetProjectById;

/// <summary>
/// Handler for GetProjectByIdQuery.
/// Returns project if user has access.
/// </summary>
public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetProjectByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ProjectDto?> Handle(
        GetProjectByIdQuery request,
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
                "You don't have permission to view this project");
        }

        // Get the project with details
        var project = await _unitOfWork.Projects.GetProjectWithDetailsAsync(
            request.ProjectId,
            cancellationToken);

        if (project == null)
        {
            return null;
        }

        // Convert to DTO
        return _mapper.Map<ProjectDto>(project);
    }
}