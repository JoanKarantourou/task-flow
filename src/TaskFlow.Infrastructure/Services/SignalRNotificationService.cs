using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Services;

/// <summary>
/// Implementation of INotificationService using SignalR for real-time notifications.
/// This service uses IHubContext to push notifications to connected clients.
/// </summary>
/// <remarks>
/// IHubContext allows us to send messages to SignalR clients from outside the Hub class.
/// This is essential because:
/// - Hub instances are created per-connection and shouldn't be injected directly
/// - We need to send notifications from command handlers and consumers
/// - IHubContext provides a safe way to access hub functionality from any service
/// 
/// SignalR Client Methods (what we call on the frontend):
/// - ReceiveTaskNotification: For task-related updates
/// - ReceiveProjectNotification: For project-related updates
/// - ReceiveSystemNotification: For system-wide messages
/// </remarks>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<Hub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalRNotificationService"/> class.
    /// </summary>
    /// <param name="hubContext">SignalR hub context for sending messages to clients.</param>
    /// <param name="logger">Logger for tracking notification delivery.</param>
    public SignalRNotificationService(
        IHubContext<Hub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task NotifyTaskCreatedAsync(
        Guid taskId,
        string taskTitle,
        Guid projectId,
        string projectName,
        string createdByName,
        Guid? assigneeId = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new
        {
            Type = "TaskCreated",
            TaskId = taskId,
            Title = $"New Task: {taskTitle}",
            Message = $"{createdByName} created a new task '{taskTitle}' in project '{projectName}'",
            ProjectId = projectId,
            ProjectName = projectName,
            Timestamp = DateTime.UtcNow,
            ActionUrl = $"/tasks/{taskId}" // Frontend can navigate to this URL
        };

        try
        {
            // Send to all members of the project group
            await _hubContext.Clients
                .Group($"project-{projectId}")
                .SendAsync("ReceiveTaskNotification", notification, cancellationToken);

            // If there's an assignee, send a direct notification to them as well
            if (assigneeId.HasValue)
            {
                await _hubContext.Clients
                    .User(assigneeId.Value.ToString())
                    .SendAsync("ReceiveTaskNotification", notification, cancellationToken);
            }

            _logger.LogInformation(
                "Task created notification sent. TaskId={TaskId}, ProjectId={ProjectId}",
                taskId,
                projectId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send task created notification. TaskId={TaskId}",
                taskId
            );
            // Don't throw - notification failures shouldn't break the main flow
        }
    }

    /// <inheritdoc/>
    public async Task NotifyTaskAssignedAsync(
        Guid taskId,
        string taskTitle,
        Guid projectId,
        string projectName,
        Guid assigneeId,
        string assigneeName,
        string assignedByName,
        CancellationToken cancellationToken = default)
    {
        var notification = new
        {
            Type = "TaskAssigned",
            TaskId = taskId,
            Title = "Task Assigned to You",
            Message = $"{assignedByName} assigned you to task '{taskTitle}' in project '{projectName}'",
            ProjectId = projectId,
            ProjectName = projectName,
            AssigneeId = assigneeId,
            AssigneeName = assigneeName,
            Timestamp = DateTime.UtcNow,
            ActionUrl = $"/tasks/{taskId}"
        };

        try
        {
            // Send direct notification to the assigned user
            await _hubContext.Clients
                .User(assigneeId.ToString())
                .SendAsync("ReceiveTaskNotification", notification, cancellationToken);

            // Also notify the project group (so others know about the assignment)
            await _hubContext.Clients
                .Group($"project-{projectId}")
                .SendAsync("ReceiveTaskNotification", notification, cancellationToken);

            _logger.LogInformation(
                "Task assigned notification sent. TaskId={TaskId}, AssigneeId={AssigneeId}",
                taskId,
                assigneeId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send task assigned notification. TaskId={TaskId}, AssigneeId={AssigneeId}",
                taskId,
                assigneeId
            );
        }
    }

    /// <inheritdoc/>
    public async Task NotifyTaskStatusChangedAsync(
        Guid taskId,
        string taskTitle,
        Guid projectId,
        string projectName,
        string oldStatus,
        string newStatus,
        string changedByName,
        Guid? assigneeId = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new
        {
            Type = "TaskStatusChanged",
            TaskId = taskId,
            Title = $"Task Status Updated: {taskTitle}",
            Message = $"{changedByName} moved task '{taskTitle}' from {oldStatus} to {newStatus}",
            ProjectId = projectId,
            ProjectName = projectName,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedBy = changedByName,
            Timestamp = DateTime.UtcNow,
            ActionUrl = $"/tasks/{taskId}"
        };

        try
        {
            // Notify project group
            await _hubContext.Clients
                .Group($"project-{projectId}")
                .SendAsync("ReceiveTaskNotification", notification, cancellationToken);

            // If there's an assignee, send them a direct notification too
            if (assigneeId.HasValue)
            {
                await _hubContext.Clients
                    .User(assigneeId.Value.ToString())
                    .SendAsync("ReceiveTaskNotification", notification, cancellationToken);
            }

            _logger.LogInformation(
                "Task status changed notification sent. TaskId={TaskId}, OldStatus={OldStatus}, NewStatus={NewStatus}",
                taskId,
                oldStatus,
                newStatus
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send task status changed notification. TaskId={TaskId}",
                taskId
            );
        }
    }

    /// <inheritdoc/>
    public async Task NotifyUserAsync(
        Guid userId,
        string title,
        string message,
        string type = "info",
        CancellationToken cancellationToken = default)
    {
        var notification = new
        {
            Type = type,
            Title = title,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .User(userId.ToString())
                .SendAsync("ReceiveSystemNotification", notification, cancellationToken);

            _logger.LogInformation(
                "User notification sent. UserId={UserId}, Type={Type}",
                userId,
                type
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send user notification. UserId={UserId}",
                userId
            );
        }
    }

    /// <inheritdoc/>
    public async Task NotifyProjectMembersAsync(
        Guid projectId,
        string title,
        string message,
        string type = "info",
        CancellationToken cancellationToken = default)
    {
        var notification = new
        {
            Type = type,
            Title = title,
            Message = message,
            ProjectId = projectId,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group($"project-{projectId}")
                .SendAsync("ReceiveProjectNotification", notification, cancellationToken);

            _logger.LogInformation(
                "Project notification sent. ProjectId={ProjectId}, Type={Type}",
                projectId,
                type
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send project notification. ProjectId={ProjectId}",
                projectId
            );
        }
    }
}