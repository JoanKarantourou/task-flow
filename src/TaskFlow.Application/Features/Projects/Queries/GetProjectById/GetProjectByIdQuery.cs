using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Projects.Queries.GetProjectById;

/// <summary>
/// Query to get a project by its ID.
/// User must have access to the project.
/// </summary>
public class GetProjectByIdQuery : IRequest<ProjectDto?>
{
    /// <summary>
    /// ID of the project to retrieve.
    /// </summary>
    public Guid ProjectId { get; set; }
}