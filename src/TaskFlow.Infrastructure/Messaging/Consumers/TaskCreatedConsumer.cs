using MassTransit;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Contracts;

namespace TaskFlow.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer that processes TaskCreatedEvent messages from RabbitMQ.
/// This consumer is responsible for handling actions that should occur when a task is created,
/// such as logging, sending notifications, or triggering other workflows.
/// 
/// MassTransit automatically routes TaskCreatedEvent messages to this consumer.
/// </summary>
public class TaskCreatedConsumer : IConsumer<TaskCreatedEvent>
{
    private readonly ILogger<TaskCreatedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskCreatedConsumer"/> class.
    /// </summary>
    /// <param name="logger">Logger for recording consumer activity and errors.</param>
    public TaskCreatedConsumer(ILogger<TaskCreatedConsumer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Consumes and processes a TaskCreatedEvent message.
    /// This method is called automatically by MassTransit when a TaskCreatedEvent is published.
    /// </summary>
    /// <param name="context">The message context containing the TaskCreatedEvent and metadata.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<TaskCreatedEvent> context)
    {
        // Extract the event data from the message context
        var message = context.Message;

        // Log the task creation event with relevant details
        _logger.LogInformation(
            "Task Created Event Received: TaskId={TaskId}, Title={Title}, Project={ProjectName}, CreatedBy={CreatedByName}",
            message.TaskId,
            message.Title,
            message.ProjectName,
            message.CreatedByName
        );

        // Simulate sending an email notification if the task is assigned to someone
        if (message.AssigneeId.HasValue && !string.IsNullOrEmpty(message.AssigneeEmail))
        {
            _logger.LogInformation(
                "Sending email notification to {AssigneeEmail}: New task '{Title}' has been assigned to you in project '{ProjectName}'",
                message.AssigneeEmail,
                message.Title,
                message.ProjectName
            );

            // In a real application, you would call an email service here
            // Example: await _emailService.SendTaskCreatedEmailAsync(message);

            // For now, we just simulate the email send with a small delay
            await Task.Delay(100); // Simulate async email operation
        }

        // Log successful processing
        _logger.LogInformation(
            "Task Created Event processed successfully for TaskId={TaskId}",
            message.TaskId
        );

        // Note: If this method throws an exception, MassTransit will automatically:
        // 1. Retry the message processing based on configured retry policy
        // 2. Move the message to an error queue if all retries fail
        // This ensures reliable message processing
    }
}