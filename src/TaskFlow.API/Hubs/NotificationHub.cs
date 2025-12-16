using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TaskFlow.API.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications.
/// Clients connect to this hub to receive instant updates about tasks, projects, and system events.
/// 
/// SignalR Hub Flow:
/// 1. Client connects to hub endpoint (/hubs/notifications)
/// 2. Client can join specific groups (e.g., project-specific notifications)
/// 3. Server pushes notifications to connected clients or groups
/// 4. Client receives and displays notifications in real-time
/// </summary>
/// <remarks>
/// This hub requires authentication. Only authenticated users can connect.
/// Users can join groups based on projects they have access to.
/// 
/// Hub Methods (called by clients):
/// - JoinProjectGroup: Subscribe to notifications for a specific project
/// - LeaveProjectGroup: Unsubscribe from project notifications
/// 
/// Client Methods (called by server):
/// - ReceiveTaskNotification: Receive task-related notifications
/// - ReceiveProjectNotification: Receive project-related notifications
/// - ReceiveSystemNotification: Receive system-wide notifications
/// </remarks>
[Authorize] // Require authentication to connect to this hub
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationHub"/> class.
    /// </summary>
    /// <param name="logger">Logger for tracking hub operations and debugging.</param>
    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client successfully connects to the hub.
    /// Logs the connection for monitoring and debugging.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task OnConnectedAsync()
    {
        // Get the user's identifier from the JWT token claims
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        _logger.LogInformation(
            "Client connected to NotificationHub. UserId={UserId}, ConnectionId={ConnectionId}",
            userId,
            connectionId
        );

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// Cleans up resources and logs the disconnection.
    /// </summary>
    /// <param name="exception">Exception that caused the disconnection, if any.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        if (exception != null)
        {
            _logger.LogWarning(
                exception,
                "Client disconnected with error. UserId={UserId}, ConnectionId={ConnectionId}",
                userId,
                connectionId
            );
        }
        else
        {
            _logger.LogInformation(
                "Client disconnected from NotificationHub. UserId={UserId}, ConnectionId={ConnectionId}",
                userId,
                connectionId
            );
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Allows a client to join a project-specific notification group.
    /// Once joined, the client will receive all notifications related to that project.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project to join.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Groups in SignalR allow broadcasting messages to specific subsets of connected clients.
    /// This is useful for project-specific notifications where only team members should be notified.
    /// 
    /// Example usage from client:
    /// await connection.InvokeAsync("JoinProjectGroup", projectId);
    /// </remarks>
    public async Task JoinProjectGroup(string projectId)
    {
        // Add this connection to the project-specific group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project-{projectId}");

        _logger.LogInformation(
            "User joined project group. UserId={UserId}, ConnectionId={ConnectionId}, ProjectId={ProjectId}",
            Context.UserIdentifier,
            Context.ConnectionId,
            projectId
        );

        // Optionally, send a confirmation message back to the client
        await Clients.Caller.SendAsync(
            "JoinedProjectGroup",
            new { ProjectId = projectId, Message = $"Successfully joined project {projectId} notifications" }
        );
    }

    /// <summary>
    /// Allows a client to leave a project-specific notification group.
    /// The client will no longer receive notifications for that project.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project to leave.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Example usage from client:
    /// await connection.InvokeAsync("LeaveProjectGroup", projectId);
    /// </remarks>
    public async Task LeaveProjectGroup(string projectId)
    {
        // Remove this connection from the project-specific group
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project-{projectId}");

        _logger.LogInformation(
            "User left project group. UserId={UserId}, ConnectionId={ConnectionId}, ProjectId={ProjectId}",
            Context.UserIdentifier,
            Context.ConnectionId,
            projectId
        );

        // Optionally, send a confirmation message back to the client
        await Clients.Caller.SendAsync(
            "LeftProjectGroup",
            new { ProjectId = projectId, Message = $"Successfully left project {projectId} notifications" }
        );
    }

    /// <summary>
    /// Sends a test notification to the calling client.
    /// Useful for testing the SignalR connection without triggering actual events.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Example usage from client:
    /// await connection.InvokeAsync("SendTestNotification");
    /// </remarks>
    public async Task SendTestNotification()
    {
        await Clients.Caller.SendAsync(
            "ReceiveTaskNotification",
            new
            {
                Type = "Test",
                Title = "Test Notification",
                Message = "This is a test notification from SignalR",
                Timestamp = DateTime.UtcNow
            }
        );

        _logger.LogInformation(
            "Test notification sent. UserId={UserId}, ConnectionId={ConnectionId}",
            Context.UserIdentifier,
            Context.ConnectionId
        );
    }
}