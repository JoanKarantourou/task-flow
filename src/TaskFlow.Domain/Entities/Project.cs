using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Represents a project that contains tasks and team members.
/// Projects are the main organizational unit for work in TaskFlow.
/// </summary>
public class Project : BaseEntity
{
    /// <summary>
    /// Name of the project.
    /// Should be descriptive and unique within the user's projects.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the project's goals and scope.
    /// Optional - can be null or empty.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Current lifecycle status of the project.
    /// Determines if work is active, paused, or complete.
    /// </summary>
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;

    /// <summary>
    /// ID of the user who owns/created this project.
    /// Owner has full control over the project.
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// When the project work is scheduled to begin.
    /// Optional - some projects may not have a defined start date.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Target completion date for the project.
    /// Optional - helps with planning and prioritization.
    /// </summary>
    public DateTime? DueDate { get; set; }

    // Navigation Properties

    /// <summary>
    /// Reference to the user who owns this project.
    /// Owner can manage project settings and members.
    /// </summary>
    public User Owner { get; set; } = null!;

    /// <summary>
    /// Collection of all tasks within this project.
    /// A project can have many tasks.
    /// </summary>
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    /// <summary>
    /// Collection of project members (users with access).
    /// Represents the team working on this project.
    /// </summary>
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
}