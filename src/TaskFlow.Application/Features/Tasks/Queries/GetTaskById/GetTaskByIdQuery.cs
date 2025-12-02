using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Features.Tasks.Commands.CreateTask;

namespace TaskFlow.Application.Features.Tasks.Queries.GetTaskById;

/// <summary>
/// Query to retrieve a single task by its unique identifier.
/// This is a Query (not a Command) because it only reads data, doesn't modify it.
/// In CQRS:
/// - Queries = Read operations (SELECT)
/// - Commands = Write operations (INSERT, UPDATE, DELETE)
/// </summary>
/// <remarks>
/// Queries are typically simpler than commands:
/// - No validation needed (just an ID)
/// - No business logic (just data retrieval)
/// - No side effects (doesn't change database state)
/// </remarks>
public class GetTaskByIdQuery : IRequest<TaskDto?>
{
    /// <summary>
    /// The unique identifier of the task to retrieve.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Constructor to create the query with a task ID.
    /// Makes it easy to create: new GetTaskByIdQuery { TaskId = id }
    /// </summary>
    public GetTaskByIdQuery(Guid taskId)
    {
        TaskId = taskId;
    }

    /// <summary>
    /// Parameterless constructor for model binding in controllers.
    /// </summary>
    public GetTaskByIdQuery()
    {
    }
}