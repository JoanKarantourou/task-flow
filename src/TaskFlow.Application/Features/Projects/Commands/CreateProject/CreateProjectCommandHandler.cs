using AutoMapper;
using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.Commands.CreateProject;

/// <summary>
/// Handler for CreateProjectCommand.
/// Creates a new project with the current user as owner.
/// </summary>
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateProjectCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ProjectDto> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to create projects");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Create the project entity
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            Status = request.Status,
            OwnerId = currentUserId // Current user becomes the owner
        };

        // Add project to repository
        await _unitOfWork.Projects.AddAsync(project, cancellationToken);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch the project with details (includes owner)
        var createdProject = await _unitOfWork.Projects.GetProjectWithDetailsAsync(
            project.Id,
            cancellationToken);

        // Convert to DTO
        var projectDto = _mapper.Map<ProjectDto>(createdProject);

        // TODO: Phase 6 - Publish ProjectCreatedEvent

        return projectDto;
    }
}