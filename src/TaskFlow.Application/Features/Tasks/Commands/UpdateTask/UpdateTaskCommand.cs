using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Features.Tasks.Commands.UpdateTask;

/// <summary>
/// Command to update an existing task.
/// User must have permission to modify the task.
/// </summary>
public class UpdateTaskCommand : IRequest<TaskDto>
{
    /// <summary>
    /// ID of the task to update.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Updated task title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Updated task description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Updated task status.
    /// </summary>
    public TaskStatus? Status { get; set; }

    /// <summary>
    /// Updated task priority.
    /// </summary>
    public TaskPriority? Priority { get; set; }

    /// <summary>
    /// Updated assignee ID (null to unassign).
    /// </summary>
    public Guid? AssigneeId { get; set; }

    /// <summary>
    /// Updated due date (null to remove due date).
    /// </summary>
    public DateTime? DueDate { get; set; }
}