namespace TaskFlow.Application.Contracts;

/// <summary>
/// Event published when a task is assigned to a user.
/// This event triggers notifications to inform the assigned user about their new task.
/// </summary>
public class TaskAssignedEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the task being assigned.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets the title/name of the task.
    /// </summary>
    public string TaskTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the user to whom the task is assigned.
    /// </summary>
    public Guid AssigneeId { get; set; }

    /// <summary>
    /// Gets or sets the full name of the assigned user.
    /// Used in notification messages to personalize the communication.
    /// </summary>
    public string AssigneeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the assigned user.
    /// Used for sending email notifications.
    /// </summary>
    public string AssigneeEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the user who assigned the task.
    /// </summary>
    public Guid AssignedBy { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who assigned the task.
    /// Included in notification messages for context.
    /// </summary>
    public string AssignedByName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the project this task belongs to.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the name of the project.
    /// Provides context in notifications about which project the task belongs to.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the assignment occurred.
    /// </summary>
    public DateTime AssignedAt { get; set; }
}