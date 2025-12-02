using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.DTOs;

/// <summary>
/// Data Transfer Object for Task entity.
/// Used for API responses when returning task information.
/// Contains all relevant task data plus related entity names for display.
/// </summary>
public class TaskDto
{
    /// <summary>
    /// Unique identifier of the task.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Task title - short description of what needs to be done.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of task requirements.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Current workflow status (Todo, InProgress, InReview, Done, Cancelled).
    /// </summary>
    public TaskStatus Status { get; set; }

    /// <summary>
    /// Priority level (Low, Medium, High, Critical).
    /// </summary>
    public TaskPriority Priority { get; set; }

    /// <summary>
    /// ID of the project this task belongs to.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Name of the project (for display without additional query).
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// ID of the assigned user (null if unassigned).
    /// </summary>
    public Guid? AssigneeId { get; set; }

    /// <summary>
    /// Full name of the assigned user (null if unassigned).
    /// </summary>
    public string? AssigneeName { get; set; }

    /// <summary>
    /// Target completion date.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// When the task was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the task was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Number of comments on this task.
    /// Useful for UI display without loading all comments.
    /// </summary>
    public int CommentCount { get; set; }
}

/// <summary>
/// Simplified DTO for task lists where full details aren't needed.
/// Used in lists, Kanban boards, and search results for better performance.
/// Reduces payload size when returning many tasks.
/// </summary>
public class TaskSummaryDto
{
    /// <summary>
    /// Unique identifier of the task.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Task title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Current workflow status.
    /// </summary>
    public TaskStatus Status { get; set; }

    /// <summary>
    /// Priority level.
    /// </summary>
    public TaskPriority Priority { get; set; }

    /// <summary>
    /// Full name of the assigned user (null if unassigned).
    /// </summary>
    public string? AssigneeName { get; set; }

    /// <summary>
    /// Target completion date.
    /// </summary>
    public DateTime? DueDate { get; set; }
}
