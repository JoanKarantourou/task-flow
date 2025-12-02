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
/// Contains the business logic for creating a new task.
/// MediatR automatically calls this when a CreateTaskCommand is sent.
/// Now uses AutoMapper for entity-to-DTO conversion.
/// </summary>
public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// Constructor - MediatR and DI will automatically inject dependencies.
    /// </summary>
    /// <param name="unitOfWork">Unit of Work for database operations</param>
    /// <param name="mapper">AutoMapper for entity-to-DTO conversion</param>
    public CreateTaskCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the CreateTaskCommand and returns the created task as a DTO.
    /// This method is called automatically by MediatR when the command is sent.
    /// </summary>
    /// <param name="request">The command containing task creation data</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The created task as a TaskDto</returns>
    /// <exception cref="ArgumentException">Thrown when project doesn't exist</exception>
    public async Task<TaskDto> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Validate that the project exists
        var projectExists = await _unitOfWork.Projects.AnyAsync(
            p => p.Id == request.ProjectId,
            cancellationToken);

        if (!projectExists)
        {
            throw new ArgumentException(
                $"Project with ID {request.ProjectId} does not exist.",
                nameof(request.ProjectId));
        }

        // Step 2: Create the domain entity
        var taskItem = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            Priority = request.Priority,
            AssigneeId = request.AssigneeId,
            DueDate = request.DueDate,
            Status = TaskStatus.Todo, // New tasks always start as Todo
        };

        // Step 3: Add the task to the repository
        await _unitOfWork.Tasks.AddAsync(taskItem, cancellationToken);

        // Step 4: Save changes to the database
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 5: Fetch the task with related data
        var createdTask = await _unitOfWork.Tasks.GetTaskWithDetailsAsync(
            taskItem.Id,
            cancellationToken);

        // Step 6: Use AutoMapper to convert entity to DTO
        // This replaces 15+ lines of manual mapping with one line!
        var taskDto = _mapper.Map<TaskDto>(createdTask);

        // TODO: Later, we'll publish a TaskCreatedEvent here for real-time notifications
        // await _publishEndpoint.Publish(new TaskCreatedEvent { ... });

        return taskDto;
    }
}