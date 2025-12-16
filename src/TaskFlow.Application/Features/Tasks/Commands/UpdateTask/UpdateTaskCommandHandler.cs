using AutoMapper;
using MassTransit;
using MediatR;
using TaskFlow.Application.Contracts;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.Commands.UpdateTask;

/// <summary>
/// Handles the UpdateTaskCommand by updating an existing task in the database
/// and publishing relevant events (TaskStatusChangedEvent, TaskAssignedEvent) to RabbitMQ.
/// </summary>
public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGenericRepository<User> _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Unit of work for database operations and transaction management.</param>
    /// <param name="mapper">AutoMapper instance for object mapping.</param>
    /// <param name="publishEndpoint">MassTransit publish endpoint for sending events to RabbitMQ.</param>
    /// <param name="currentUserService">Service to get the current authenticated user.</param>
    /// <param name="userRepository">Generic repository for accessing User entities.</param>
    public UpdateTaskCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPublishEndpoint publishEndpoint,
        ICurrentUserService currentUserService,
        IGenericRepository<User> userRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Handles the UpdateTaskCommand by:
    /// 1. Retrieving the existing task
    /// 2. Detecting changes (status, assignment)
    /// 3. Updating the task
    /// 4. Publishing appropriate events to RabbitMQ
    /// </summary>
    /// <param name="request">The update task command containing task details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A TaskDto representing the updated task.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the task is not found.</exception>
    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        // Get the current user ID (property, not method)
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to update tasks");

        // Retrieve the existing task from the database (using TaskId property from command)
        var task = await _unitOfWork.Tasks.GetByIdAsync(request.TaskId);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.TaskId} not found");
        }

        // Store old values to detect changes
        var oldStatus = task.Status;
        var oldAssigneeId = task.AssigneeId;

        // Update task properties (all properties are nullable in UpdateTaskCommand)
        if (request.Title != null)
            task.Title = request.Title;

        if (request.Description != null)
            task.Description = request.Description;

        if (request.Status.HasValue)
            task.Status = request.Status.Value;

        if (request.Priority.HasValue)
            task.Priority = request.Priority.Value;

        // AssigneeId can be explicitly set to null to unassign
        task.AssigneeId = request.AssigneeId;

        if (request.DueDate.HasValue)
            task.DueDate = request.DueDate;

        // Update the task in the repository
        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch related entities for event publishing
        var project = await _unitOfWork.Projects.GetByIdAsync(task.ProjectId);
        var currentUser = await _userRepository.GetByIdAsync(currentUserId);
        var currentUserName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "Unknown User";

        // === Publish TaskStatusChangedEvent if status changed ===
        if (request.Status.HasValue && oldStatus != task.Status)
        {
            User? assignee = null;
            if (task.AssigneeId.HasValue)
            {
                assignee = await _userRepository.GetByIdAsync(task.AssigneeId.Value);
            }

            await _publishEndpoint.Publish(new TaskStatusChangedEvent
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                OldStatus = oldStatus,
                NewStatus = task.Status,
                ChangedBy = currentUserId,
                ChangedByName = currentUserName,
                AssigneeId = task.AssigneeId,
                AssigneeEmail = assignee?.Email,
                ProjectId = task.ProjectId,
                ProjectName = project?.Name ?? "Unknown Project",
                ChangedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        // === Publish TaskAssignedEvent if assignment changed ===
        if (oldAssigneeId != task.AssigneeId && task.AssigneeId.HasValue)
        {
            var newAssignee = await _userRepository.GetByIdAsync(task.AssigneeId.Value);

            if (newAssignee != null)
            {
                await _publishEndpoint.Publish(new TaskAssignedEvent
                {
                    TaskId = task.Id,
                    TaskTitle = task.Title,
                    AssigneeId = newAssignee.Id,
                    AssigneeName = $"{newAssignee.FirstName} {newAssignee.LastName}",
                    AssigneeEmail = newAssignee.Email,
                    AssignedBy = currentUserId,
                    AssignedByName = currentUserName,
                    ProjectId = task.ProjectId,
                    ProjectName = project?.Name ?? "Unknown Project",
                    AssignedAt = DateTime.UtcNow
                }, cancellationToken);
            }
        }

        // Map the entity to DTO and return
        return _mapper.Map<TaskDto>(task);
    }
}