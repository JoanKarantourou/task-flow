using MassTransit;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Contracts;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer that processes TaskStatusChangedEvent messages from RabbitMQ.
/// Handles logging, email notifications, and real-time SignalR notifications.
/// </summary>
public class TaskStatusChangedConsumer : IConsumer<TaskStatusChangedEvent>
{
    private readonly ILogger<TaskStatusChangedConsumer> _logger;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskStatusChangedConsumer"/> class.
    /// </summary>
    /// <param name="logger">Logger for recording consumer activity and errors.</param>
    /// <param name="notificationService">Service for sending real-time SignalR notifications.</param>
    public TaskStatusChangedConsumer(
        ILogger<TaskStatusChangedConsumer> logger,
        INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Consumes and processes a TaskStatusChangedEvent message.
    /// This method is called automatically by MassTransit when a TaskStatusChangedEvent is published.
    /// </summary>
    /// <param name="context">The message context containing the TaskStatusChangedEvent and metadata.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<TaskStatusChangedEvent> context)
    {
        var message = context.Message;

        // Log the status change event
        _logger.LogInformation(
            "Task Status Changed Event Received: TaskId={TaskId}, Title={TaskTitle}, OldStatus={OldStatus}, NewStatus={NewStatus}, ChangedBy={ChangedByName}, Project={ProjectName}",
            message.TaskId,
            message.TaskTitle,
            message.OldStatus,
            message.NewStatus,
            message.ChangedByName,
            message.ProjectName
        );

        // Send real-time SignalR notification to connected clients
        await _notificationService.NotifyTaskStatusChangedAsync(
            message.TaskId,
            message.TaskTitle,
            message.ProjectId,
            message.ProjectName,
            message.OldStatus.ToString(),
            message.NewStatus.ToString(),
            message.ChangedByName,
            message.AssigneeId,
            context.CancellationToken
        );

        // Determine if this is an important status change that requires notification
        bool isImportantChange = IsImportantStatusChange(message.OldStatus, message.NewStatus);

        // Send email notification if the change is important and there's an assignee
        if (message.AssigneeId.HasValue &&
            !string.IsNullOrEmpty(message.AssigneeEmail) &&
            isImportantChange)
        {
            _logger.LogInformation(
                "Sending status change notification to {AssigneeEmail}: Task '{TaskTitle}' status changed from {OldStatus} to {NewStatus} by {ChangedByName}",
                message.AssigneeEmail,
                message.TaskTitle,
                message.OldStatus,
                message.NewStatus,
                message.ChangedByName
            );

            await Task.Delay(100); // Simulate async email operation
        }

        // Log successful processing
        _logger.LogInformation(
            "Task Status Changed Event processed successfully for TaskId={TaskId}",
            message.TaskId
        );
    }

    /// <summary>
    /// Determines if a status change is significant enough to warrant notifications.
    /// </summary>
    private bool IsImportantStatusChange(Domain.Enums.TaskStatus oldStatus, Domain.Enums.TaskStatus newStatus)
    {
        return newStatus == Domain.Enums.TaskStatus.Done ||
               oldStatus == Domain.Enums.TaskStatus.Done ||
               newStatus == Domain.Enums.TaskStatus.InReview ||
               (oldStatus == Domain.Enums.TaskStatus.InProgress && newStatus != Domain.Enums.TaskStatus.InReview);
    }
}