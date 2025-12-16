using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Projects.Commands.CreateProject;

/// <summary>
/// Command to create a new project.
/// Current user will automatically become the project owner.
/// </summary>
public class CreateProjectCommand : IRequest<ProjectDto>
{
    /// <summary>
    /// Project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Project description (optional).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Project start date (optional).
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Project due date (optional).
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Project status (defaults to Active if not specified).
    /// </summary>
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
}