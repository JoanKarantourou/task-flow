using AutoMapper;
using MassTransit;
using MediatR;
using TaskFlow.Application.Contracts;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Features.Tasks.Commands.CreateTask;

/// <summary>
/// Handles the CreateTaskCommand by creating a new task in the database
/// and publishing a TaskCreatedEvent to RabbitMQ for downstream processing.
/// </summary>
public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGenericRepository<User> _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTaskCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Unit of work for database operations and transaction management.</param>
    /// <param name="mapper">AutoMapper instance for object mapping.</param>
    /// <param name="publishEndpoint">MassTransit publish endpoint for sending events to RabbitMQ.</param>
    /// <param name="currentUserService">Service to get the current authenticated user.</param>
    /// <param name="userRepository">Generic repository for accessing User entities.</param>
    public CreateTaskCommandHandler(
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
    /// Handles the CreateTaskCommand by:
    /// 1. Creating the task entity
    /// 2. Saving it to the database
    /// 3. Publishing a TaskCreatedEvent to RabbitMQ
    /// </summary>
    /// <param name="request">The create task command containing task details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A TaskDto representing the created task.</returns>
    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        // Get the current user ID from the authentication context (property, not method)
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to create tasks");

        // Create the task entity from the command
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            AssigneeId = request.AssigneeId,
            Status = TaskStatus.Todo, // New tasks always start in Todo status
            Priority = request.Priority,
            DueDate = request.DueDate
        };

        // Add the task to the repository
        await _unitOfWork.Tasks.AddAsync(task);

        // Save changes to the database to get the generated TaskId
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch the related entities for the event using the injected user repository
        var project = await _unitOfWork.Projects.GetByIdAsync(task.ProjectId);
        var creator = await _userRepository.GetByIdAsync(currentUserId);
        User? assignee = null;

        if (task.AssigneeId.HasValue)
        {
            assignee = await _userRepository.GetByIdAsync(task.AssigneeId.Value);
        }

        // Publish TaskCreatedEvent to RabbitMQ
        // This event will be consumed by TaskCreatedConsumer for notifications and logging
        await _publishEndpoint.Publish(new TaskCreatedEvent
        {
            TaskId = task.Id,
            Title = task.Title,
            ProjectId = task.ProjectId,
            ProjectName = project?.Name ?? "Unknown Project",
            CreatedBy = currentUserId,
            CreatedByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown User",
            AssigneeId = task.AssigneeId,
            AssigneeEmail = assignee?.Email,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        // Map the entity to DTO and return
        return _mapper.Map<TaskDto>(task);
    }
}