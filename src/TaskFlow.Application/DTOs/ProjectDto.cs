using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs;

/// <summary>
/// Data Transfer Object for Project entity.
/// Used for API responses when returning project information.
/// Contains project details plus summary statistics.
/// </summary>
public class ProjectDto
{
    /// <summary>
    /// Unique identifier of the project.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of project goals and scope.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Current lifecycle status (Active, OnHold, Completed, Archived).
    /// </summary>
    public ProjectStatus Status { get; set; }

    /// <summary>
    /// ID of the user who owns this project.
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Full name of the project owner.
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;

    /// <summary>
    /// When the project work is scheduled to begin.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Target completion date for the project.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// When the project was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the project was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Total number of tasks in this project.
    /// </summary>
    public int TotalTasks { get; set; }

    /// <summary>
    /// Number of completed tasks.
    /// </summary>
    public int CompletedTasks { get; set; }

    /// <summary>
    /// Number of team members (excluding owner).
    /// </summary>
    public int MemberCount { get; set; }
}

/// <summary>
/// Simplified DTO for project lists where full details aren't needed.
/// Used in dashboards, dropdowns, and search results.
/// </summary>
public class ProjectSummaryDto
{
    /// <summary>
    /// Unique identifier of the project.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Current lifecycle status.
    /// </summary>
    public ProjectStatus Status { get; set; }

    /// <summary>
    /// Full name of the project owner.
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of tasks in this project.
    /// </summary>
    public int TotalTasks { get; set; }

    /// <summary>
    /// Number of completed tasks.
    /// </summary>
    public int CompletedTasks { get; set; }
}

/// <summary>
/// Detailed project DTO including task lists and members.
/// Used when full project details with relationships are needed.
/// More expensive to generate but provides complete picture.
/// </summary>
public class ProjectDetailsDto : ProjectDto
{
    /// <summary>
    /// List of tasks in this project.
    /// Uses TaskSummaryDto to avoid deep nesting.
    /// </summary>
    public List<TaskSummaryDto> Tasks { get; set; } = new();

    /// <summary>
    /// List of project members.
    /// </summary>
    public List<ProjectMemberDto> Members { get; set; } = new();
}
