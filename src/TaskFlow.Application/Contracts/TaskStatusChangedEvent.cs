using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Contracts;

/// <summary>
/// Event published when a task's status changes (e.g., from Todo to InProgress, or InProgress to Done).
/// This event allows the system to track task progress and notify relevant stakeholders.
/// </summary>
public class TaskStatusChangedEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the task whose status changed.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets the title/name of the task.
    /// </summary>
    public string TaskTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the previous status of the task before the change.
    /// </summary>
    public TaskStatus OldStatus { get; set; }

    /// <summary>
    /// Gets or sets the new status of the task after the change.
    /// </summary>
    public TaskStatus NewStatus { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who changed the status.
    /// </summary>
    public Guid ChangedBy { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who changed the status.
    /// </summary>
    public string ChangedByName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the user assigned to this task (if any).
    /// Null if the task is unassigned.
    /// </summary>
    public Guid? AssigneeId { get; set; }

    /// <summary>
    /// Gets or sets the email of the assigned user (if any).
    /// Used for sending status change notifications.
    /// </summary>
    public string? AssigneeEmail { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the project this task belongs to.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the name of the project.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the status change occurred.
    /// </summary>
    public DateTime ChangedAt { get; set; }
}