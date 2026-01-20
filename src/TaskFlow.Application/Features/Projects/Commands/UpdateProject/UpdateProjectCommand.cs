using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Projects.Commands.UpdateProject;

/// <summary>
/// Command to update an existing project.
/// User must be the project owner to update.
/// </summary>
public class UpdateProjectCommand : IRequest<ProjectDto>
{
    /// <summary>
    /// ID of the project to update.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Updated project name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Updated project description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Updated project status.
    /// </summary>
    public ProjectStatus? Status { get; set; }

    /// <summary>
    /// Updated start date.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Updated due date.
    /// </summary>
    public DateTime? DueDate { get; set; }
}
