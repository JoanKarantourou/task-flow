namespace TaskFlow.Application.Contracts;

/// <summary>
/// Event published when a new task is created in the system.
/// This event is consumed by message handlers that need to react to task creation,
/// such as sending notifications, logging, or triggering other workflows.
/// </summary>
public class TaskCreatedEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the created task.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets the title/name of the task.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the project this task belongs to.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the name of the project this task belongs to.
    /// Included for convenience in consumers to avoid additional database lookups.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the user who created the task.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who created the task.
    /// Included for convenience in notification messages.
    /// </summary>
    public string CreatedByName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the assigned user (if any).
    /// Null if the task is not yet assigned to anyone.
    /// </summary>
    public Guid? AssigneeId { get; set; }

    /// <summary>
    /// Gets or sets the email address of the assigned user (if any).
    /// Used for sending email notifications to the assignee.
    /// </summary>
    public string? AssigneeEmail { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the task was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}