using AutoMapper;
using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Features.Projects.Commands.UpdateProject;

/// <summary>
/// Handler for UpdateProjectCommand.
/// Updates an existing project owned by the current user.
/// </summary>
public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProjectCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ProjectDto> Handle(
        UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to update projects");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Get the project
        var project = await _unitOfWork.Projects.GetProjectWithDetailsAsync(
            request.ProjectId,
            cancellationToken);

        if (project == null)
        {
            throw new ArgumentException($"Project with ID {request.ProjectId} not found");
        }

        // Check if user is the owner
        if (project.OwnerId != currentUserId)
        {
            throw new UnauthorizedAccessException("Only the project owner can update this project");
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            project.Name = request.Name;
        }

        if (request.Description != null)
        {
            project.Description = request.Description;
        }

        if (request.Status.HasValue)
        {
            project.Status = request.Status.Value;
        }

        if (request.StartDate.HasValue)
        {
            project.StartDate = request.StartDate.Value;
        }

        if (request.DueDate.HasValue)
        {
            project.DueDate = request.DueDate.Value;
        }

        // Update timestamps
        project.UpdatedAt = DateTime.UtcNow;

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch updated project with details
        var updatedProject = await _unitOfWork.Projects.GetProjectWithDetailsAsync(
            project.Id,
            cancellationToken);

        return _mapper.Map<ProjectDto>(updatedProject);
    }
}
