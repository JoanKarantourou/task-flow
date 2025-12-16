using MassTransit;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Contracts;

namespace TaskFlow.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer that processes TaskAssignedEvent messages from RabbitMQ.
/// This consumer handles notifications and logging when a task is assigned to a user.
/// 
/// This is typically triggered when:
/// - A task is initially assigned during creation
/// - A task is reassigned from one user to another
/// - An unassigned task is assigned to a user
/// </summary>
public class TaskAssignedConsumer : IConsumer<TaskAssignedEvent>
{
    private readonly ILogger<TaskAssignedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskAssignedConsumer"/> class.
    /// </summary>
    /// <param name="logger">Logger for recording consumer activity and errors.</param>
    public TaskAssignedConsumer(ILogger<TaskAssignedConsumer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Consumes and processes a TaskAssignedEvent message.
    /// This method is called automatically by MassTransit when a TaskAssignedEvent is published.
    /// </summary>
    /// <param name="context">The message context containing the TaskAssignedEvent and metadata.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<TaskAssignedEvent> context)
    {
        // Extract the event data from the message context
        var message = context.Message;

        // Log the task assignment event with all relevant details
        _logger.LogInformation(
            "Task Assigned Event Received: TaskId={TaskId}, Title={TaskTitle}, Assignee={AssigneeName}, AssignedBy={AssignedByName}, Project={ProjectName}",
            message.TaskId,
            message.TaskTitle,
            message.AssigneeName,
            message.AssignedByName,
            message.ProjectName
        );

        // Simulate sending an email notification to the assigned user
        _logger.LogInformation(
            "Sending email notification to {AssigneeEmail}: You have been assigned to task '{TaskTitle}' in project '{ProjectName}' by {AssignedByName}",
            message.AssigneeEmail,
            message.TaskTitle,
            message.ProjectName,
            message.AssignedByName
        );

        // In a real application, you would call an email service here
        // Example: 
        // await _emailService.SendTaskAssignedEmailAsync(new TaskAssignedEmail
        // {
        //     To = message.AssigneeEmail,
        //     TaskTitle = message.TaskTitle,
        //     ProjectName = message.ProjectName,
        //     AssignedBy = message.AssignedByName,
        //     TaskUrl = $"https://taskflow.app/tasks/{message.TaskId}"
        // });

        // Simulate async email operation
        await Task.Delay(100);

        // Log successful processing
        _logger.LogInformation(
            "Task Assigned Event processed successfully for TaskId={TaskId}, Assignee={AssigneeName}",
            message.TaskId,
            message.AssigneeName
        );

        // Additional actions could include:
        // - Creating a notification record in the database
        // - Sending a push notification to mobile devices
        // - Updating user statistics (tasks assigned count)
        // - Triggering integrations with other systems (Slack, Teams, etc.)
    }
}