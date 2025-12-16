using MassTransit;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Contracts;

namespace TaskFlow.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer that processes TaskStatusChangedEvent messages from RabbitMQ.
/// This consumer handles notifications and logging when a task's status changes,
/// allowing team members to stay informed about task progress.
/// </summary>
public class TaskStatusChangedConsumer : IConsumer<TaskStatusChangedEvent>
{
    private readonly ILogger<TaskStatusChangedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskStatusChangedConsumer"/> class.
    /// </summary>
    /// <param name="logger">Logger for recording consumer activity and errors.</param>
    public TaskStatusChangedConsumer(ILogger<TaskStatusChangedConsumer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Consumes and processes a TaskStatusChangedEvent message.
    /// This method is called automatically by MassTransit when a TaskStatusChangedEvent is published.
    /// </summary>
    /// <param name="context">The message context containing the TaskStatusChangedEvent and metadata.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<TaskStatusChangedEvent> context)
    {
        // Extract the event data from the message context
        var message = context.Message;

        // Log the status change event with all relevant details
        _logger.LogInformation(
            "Task Status Changed Event Received: TaskId={TaskId}, Title={TaskTitle}, OldStatus={OldStatus}, NewStatus={NewStatus}, ChangedBy={ChangedByName}, Project={ProjectName}",
            message.TaskId,
            message.TaskTitle,
            message.OldStatus,
            message.NewStatus,
            message.ChangedByName,
            message.ProjectName
        );

        // Determine if this is an important status change that requires notification
        bool isImportantChange = IsImportantStatusChange(message.OldStatus, message.NewStatus);

        // Send notification to the assignee if the task is assigned and the change is important
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

            // In a real application, you would call an email service here
            // Example:
            // await _emailService.SendTaskStatusChangedEmailAsync(new TaskStatusChangedEmail
            // {
            //     To = message.AssigneeEmail,
            //     TaskTitle = message.TaskTitle,
            //     OldStatus = message.OldStatus.ToString(),
            //     NewStatus = message.NewStatus.ToString(),
            //     ChangedBy = message.ChangedByName,
            //     ProjectName = message.ProjectName,
            //     TaskUrl = $"https://taskflow.app/tasks/{message.TaskId}"
            // });

            // Simulate async email operation
            await Task.Delay(100);
        }

        // Log successful processing
        _logger.LogInformation(
            "Task Status Changed Event processed successfully for TaskId={TaskId}",
            message.TaskId
        );

        // Additional actions based on specific status changes:
        // - When status changes to "Done": Update project completion percentage
        // - When status changes to "InReview": Notify project manager for review
        // - When status changes to "Cancelled": Release resources, notify stakeholders
    }

    /// <summary>
    /// Determines if a status change is significant enough to warrant notifications.
    /// </summary>
    /// <param name="oldStatus">The previous status of the task.</param>
    /// <param name="newStatus">The new status of the task.</param>
    /// <returns>True if the status change is important; otherwise, false.</returns>
    private bool IsImportantStatusChange(Domain.Enums.TaskStatus oldStatus, Domain.Enums.TaskStatus newStatus)
    {
        // Consider the following as important changes:
        // - Moving to or from "Done" status
        // - Moving to "InReview" status
        // - Moving from "InProgress" to something else

        return newStatus == Domain.Enums.TaskStatus.Done ||
               oldStatus == Domain.Enums.TaskStatus.Done ||
               newStatus == Domain.Enums.TaskStatus.InReview ||
               (oldStatus == Domain.Enums.TaskStatus.InProgress && newStatus != Domain.Enums.TaskStatus.InReview);
    }
}