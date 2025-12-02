using System.Xml.Linq;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Represents an individual task within a project.
/// Named "TaskItem" to avoid conflict with System.Threading.Tasks.Task.
/// Tasks are the smallest unit of work in TaskFlow.
/// </summary>
public class TaskItem : BaseEntity
{
    /// <summary>
    /// Short, descriptive title of the task.
    /// Should clearly communicate what needs to be done.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the task requirements.
    /// Optional - can include acceptance criteria, technical details, etc.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Current workflow status of the task.
    /// Tracks progress from Todo through to Done.
    /// </summary>
    public TaskStatus Status { get; set; } = TaskStatus.Todo;

    /// <summary>
    /// Priority level indicating urgency and importance.
    /// Helps teams decide which tasks to work on first.
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>
    /// ID of the project this task belongs to.
    /// Every task must be associated with a project.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// ID of the user assigned to work on this task.
    /// Nullable - tasks can be unassigned.
    /// </summary>
    public Guid? AssigneeId { get; set; }

    /// <summary>
    /// Target completion date for the task.
    /// Optional - helps with time management and sprint planning.
    /// </summary>
    public DateTime? DueDate { get; set; }

    // Navigation Properties

    /// <summary>
    /// Reference to the parent project.
    /// Provides context about the larger body of work.
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Reference to the user assigned to this task.
    /// Null if task is unassigned and available for pickup.
    /// </summary>
    public User? Assignee { get; set; }

    /// <summary>
    /// Collection of comments/discussions on this task.
    /// Allows team collaboration and status updates.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}