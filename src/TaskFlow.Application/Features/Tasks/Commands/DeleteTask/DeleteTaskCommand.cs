using MediatR;

namespace TaskFlow.Application.Features.Tasks.Commands.DeleteTask;

/// <summary>
/// Command to delete a task.
/// User must have permission to delete (project owner or task creator).
/// This will also delete all associated comments.
/// </summary>
public class DeleteTaskCommand : IRequest<bool>
{
    /// <summary>
    /// ID of the task to delete.
    /// </summary>
    public Guid TaskId { get; set; }
}
