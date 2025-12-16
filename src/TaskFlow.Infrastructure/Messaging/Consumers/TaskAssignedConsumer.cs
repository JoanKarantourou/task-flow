using MassTransit;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Contracts;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer that processes TaskAssignedEvent messages from RabbitMQ.
/// Handles logging, email notifications, and real-time SignalR notifications.
/// </summary>
public class TaskAssignedConsumer : IConsumer<TaskAssignedEvent>
{
    private readonly ILogger<TaskAssignedConsumer> _logger;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskAssignedConsumer"/> class.
    /// </summary>
    /// <param name="logger">Logger for recording consumer activity and errors.</param>
    /// <param name="notificationService">Service for sending real-time SignalR notifications.</param>
    public TaskAssignedConsumer(
        ILogger<TaskAssignedConsumer> logger,
        INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Consumes and processes a TaskAssignedEvent message.
    /// This method is called automatically by MassTransit when a TaskAssignedEvent is published.
    /// </summary>
    /// <param name="context">The message context containing the TaskAssignedEvent and metadata.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<TaskAssignedEvent> context)
    {
        var message = context.Message;

        // Log the task assignment event
        _logger.LogInformation(
            "Task Assigned Event Received: TaskId={TaskId}, Title={TaskTitle}, Assignee={AssigneeName}, AssignedBy={AssignedByName}, Project={ProjectName}",
            message.TaskId,
            message.TaskTitle,
            message.AssigneeName,
            message.AssignedByName,
            message.ProjectName
        );

        // Send real-time SignalR notification to connected clients
        await _notificationService.NotifyTaskAssignedAsync(
            message.TaskId,
            message.TaskTitle,
            message.ProjectId,
            message.ProjectName,
            message.AssigneeId,
            message.AssigneeName,
            message.AssignedByName,
            context.CancellationToken
        );

        // Simulate sending an email notification to the assigned user
        _logger.LogInformation(
            "Sending email notification to {AssigneeEmail}: You have been assigned to task '{TaskTitle}' in project '{ProjectName}' by {AssignedByName}",
            message.AssigneeEmail,
            message.TaskTitle,
            message.ProjectName,
            message.AssignedByName
        );

        await Task.Delay(100); // Simulate async email operation

        // Log successful processing
        _logger.LogInformation(
            "Task Assigned Event processed successfully for TaskId={TaskId}, Assignee={AssigneeName}",
            message.TaskId,
            message.AssigneeName
        );
    }
}