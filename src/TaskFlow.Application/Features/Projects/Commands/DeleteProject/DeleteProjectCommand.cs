using MediatR;

namespace TaskFlow.Application.Features.Projects.Commands.DeleteProject;

/// <summary>
/// Command to delete a project.
/// User must be the project owner to delete.
/// This will also delete all associated tasks and comments.
/// </summary>
public class DeleteProjectCommand : IRequest<bool>
{
    /// <summary>
    /// ID of the project to delete.
    /// </summary>
    public Guid ProjectId { get; set; }
}
