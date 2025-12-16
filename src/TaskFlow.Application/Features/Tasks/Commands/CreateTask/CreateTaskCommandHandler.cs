using AutoMapper;
using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Features.Tasks.Commands.CreateTask;

/// <summary>
/// Handler for CreateTaskCommand.
/// Contains the business logic for creating a new task with authorization checks.
/// </summary>
public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Constructor - DI injects dependencies.
    /// </summary>
    public CreateTaskCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Handles the CreateTaskCommand and returns the created task.
    /// Validates that:
    /// 1. User is authenticated
    /// 2. Project exists
    /// 3. User has access to the project (is owner or member)
    /// </summary>
    public async Task<TaskDto> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Check if user is authenticated
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to create tasks");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Step 2: Validate that the project exists
        var project = await _unitOfWork.Projects.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new ArgumentException(
                $"Project with ID {request.ProjectId} does not exist.",
                nameof(request.ProjectId));
        }

        // Step 3: Check if user has access to the project
        var hasAccess = await _unitOfWork.Projects.UserHasAccessToProjectAsync(
            request.ProjectId,
            currentUserId,
            cancellationToken);

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to create tasks in this project");
        }

        // Step 4: Validate assignee exists (if provided)
        if (request.AssigneeId.HasValue)
        {
            var assigneeExists = await _unitOfWork.Projects.UserHasAccessToProjectAsync(
                request.ProjectId,
                request.AssigneeId.Value,
                cancellationToken);

            if (!assigneeExists)
            {
                throw new ArgumentException(
                    "Cannot assign task to user who is not a member of this project",
                    nameof(request.AssigneeId));
            }
        }

        // Step 5: Create the domain entity
        var taskItem = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            Priority = request.Priority,
            AssigneeId = request.AssigneeId,
            DueDate = request.DueDate,
            Status = TaskStatus.Todo // New tasks always start as Todo
        };

        // Step 6: Add the task to the repository
        await _unitOfWork.Tasks.AddAsync(taskItem, cancellationToken);

        // Step 7: Save changes to the database
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 8: Fetch the task with related data
        var createdTask = await _unitOfWork.Tasks.GetTaskWithDetailsAsync(
            taskItem.Id,
            cancellationToken);

        // Step 9: Convert entity to DTO
        var taskDto = _mapper.Map<TaskDto>(createdTask);

        // TODO: Phase 6 - Publish TaskCreatedEvent for real-time notifications
        // await _publishEndpoint.Publish(new TaskCreatedEvent { ... });

        return taskDto;
    }
}