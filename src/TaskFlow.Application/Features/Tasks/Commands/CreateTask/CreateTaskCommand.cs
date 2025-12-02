using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Tasks.Commands.CreateTask;

/// <summary>
/// Command to create a new task in a project.
/// This represents the request/intent to create a task.
/// MediatR will route this command to its handler.
/// Implements IRequest with TaskDto as the response type.
/// </summary>
/// <remarks>
/// In CQRS pattern:
/// - Commands change state (Create, Update, Delete)
/// - This is a Command because it creates a new task
/// - The handler will process this and return the created task as a DTO
/// </remarks>
public class CreateTaskCommand : IRequest<TaskDto>
{
    /// <summary>
    /// Title of the task - short description of what needs to be done.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of task requirements and acceptance criteria.
    /// Optional - can be null or empty.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// ID of the project this task belongs to.
    /// Required - every task must be associated with a project.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Priority level of the task (Low, Medium, High, Critical).
    /// Defaults to Medium if not specified.
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>
    /// ID of the user assigned to this task.
    /// Optional - tasks can be created unassigned.
    /// </summary>
    public Guid? AssigneeId { get; set; }

    /// <summary>
    /// Target completion date for the task.
    /// Optional - not all tasks have deadlines.
    /// </summary>
    public DateTime? DueDate { get; set; }
}