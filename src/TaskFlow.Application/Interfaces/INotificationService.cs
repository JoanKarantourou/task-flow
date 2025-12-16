namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Service interface for sending real-time notifications via SignalR.
/// This abstraction allows the Application layer to send notifications
/// without depending directly on SignalR infrastructure.
/// </summary>
/// <remarks>
/// This service is implemented in the Infrastructure layer using SignalR,
/// but the Application layer only knows about this interface.
/// This maintains proper separation of concerns in Clean Architecture.
/// 
/// Typical usage in command handlers or consumers:
/// - After creating a task, notify relevant users
/// - After status change, notify assignee and project members
/// - After assignment, notify the newly assigned user
/// </remarks>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification when a new task is created.
    /// Notifies project members and the assigned user (if any).
    /// </summary>
    /// <param name="taskId">Unique identifier of the created task.</param>
    /// <param name="taskTitle">Title of the created task.</param>
    /// <param name="projectId">Unique identifier of the project the task belongs to.</param>
    /// <param name="projectName">Name of the project for display purposes.</param>
    /// <param name="createdByName">Name of the user who created the task.</param>
    /// <param name="assigneeId">Optional ID of the user assigned to the task.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task NotifyTaskCreatedAsync(
        Guid taskId,
        string taskTitle,
        Guid projectId,
        string projectName,
        string createdByName,
        Guid? assigneeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification when a task is assigned to a user.
    /// Notifies the newly assigned user and optionally project members.
    /// </summary>
    /// <param name="taskId">Unique identifier of the task.</param>
    /// <param name="taskTitle">Title of the task.</param>
    /// <param name="projectId">Unique identifier of the project.</param>
    /// <param name="projectName">Name of the project.</param>
    /// <param name="assigneeId">ID of the user assigned to the task.</param>
    /// <param name="assigneeName">Name of the assigned user.</param>
    /// <param name="assignedByName">Name of the user who made the assignment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task NotifyTaskAssignedAsync(
        Guid taskId,
        string taskTitle,
        Guid projectId,
        string projectName,
        Guid assigneeId,
        string assigneeName,
        string assignedByName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification when a task's status changes.
    /// Notifies the assignee, project members, and potentially other stakeholders.
    /// </summary>
    /// <param name="taskId">Unique identifier of the task.</param>
    /// <param name="taskTitle">Title of the task.</param>
    /// <param name="projectId">Unique identifier of the project.</param>
    /// <param name="projectName">Name of the project.</param>
    /// <param name="oldStatus">Previous status of the task.</param>
    /// <param name="newStatus">New status of the task.</param>
    /// <param name="changedByName">Name of the user who changed the status.</param>
    /// <param name="assigneeId">Optional ID of the assigned user to notify directly.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task NotifyTaskStatusChangedAsync(
        Guid taskId,
        string taskTitle,
        Guid projectId,
        string projectName,
        string oldStatus,
        string newStatus,
        string changedByName,
        Guid? assigneeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a general notification to a specific user.
    /// Can be used for various notification types not covered by specific methods.
    /// </summary>
    /// <param name="userId">ID of the user to notify.</param>
    /// <param name="title">Notification title.</param>
    /// <param name="message">Notification message body.</param>
    /// <param name="type">Type of notification (info, warning, success, error).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task NotifyUserAsync(
        Guid userId,
        string title,
        string message,
        string type = "info",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to all members of a project.
    /// Useful for project-wide announcements or updates.
    /// </summary>
    /// <param name="projectId">ID of the project whose members should be notified.</param>
    /// <param name="title">Notification title.</param>
    /// <param name="message">Notification message body.</param>
    /// <param name="type">Type of notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task NotifyProjectMembersAsync(
        Guid projectId,
        string title,
        string message,
        string type = "info",
        CancellationToken cancellationToken = default);
}