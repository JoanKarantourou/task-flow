using MassTransit;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Contracts;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer that processes TaskCreatedEvent messages from RabbitMQ.
/// Handles logging, email notifications, and real-time SignalR notifications.
/// </summary>
public class TaskCreatedConsumer : IConsumer<TaskCreatedEvent>
{
    private readonly ILogger<TaskCreatedConsumer> _logger;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskCreatedConsumer"/> class.
    /// </summary>
    /// <param name="logger">Logger for recording consumer activity and errors.</param>
    /// <param name="notificationService">Service for sending real-time SignalR notifications.</param>
    public TaskCreatedConsumer(
        ILogger<TaskCreatedConsumer> logger,
        INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Consumes and processes a TaskCreatedEvent message.
    /// This method is called automatically by MassTransit when a TaskCreatedEvent is published.
    /// </summary>
    /// <param name="context">The message context containing the TaskCreatedEvent and metadata.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<TaskCreatedEvent> context)
    {
        var message = context.Message;

        // Log the task creation event
        _logger.LogInformation(
            "Task Created Event Received: TaskId={TaskId}, Title={Title}, Project={ProjectName}, CreatedBy={CreatedByName}",
            message.TaskId,
            message.Title,
            message.ProjectName,
            message.CreatedByName
        );

        // Send real-time SignalR notification to connected clients
        await _notificationService.NotifyTaskCreatedAsync(
            message.TaskId,
            message.Title,
            message.ProjectId,
            message.ProjectName,
            message.CreatedByName,
            message.AssigneeId,
            context.CancellationToken
        );

        // Simulate sending an email notification if the task is assigned
        if (message.AssigneeId.HasValue && !string.IsNullOrEmpty(message.AssigneeEmail))
        {
            _logger.LogInformation(
                "Sending email notification to {AssigneeEmail}: New task '{Title}' has been assigned to you in project '{ProjectName}'",
                message.AssigneeEmail,
                message.Title,
                message.ProjectName
            );

            // In a real application, you would call an email service here
            await Task.Delay(100); // Simulate async email operation
        }

        // Log successful processing
        _logger.LogInformation(
            "Task Created Event processed successfully for TaskId={TaskId}",
            message.TaskId
        );
    }
}