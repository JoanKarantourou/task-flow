using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Represents a comment on a task.
/// Allows team members to discuss, provide updates, and ask questions about tasks.
/// </summary>
public class Comment : BaseEntity
{
    /// <summary>
    /// The text content of the comment.
    /// Can include status updates, questions, or additional context.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// ID of the task this comment belongs to.
    /// Every comment is associated with a specific task.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// ID of the user who wrote this comment.
    /// Used to track who said what and when.
    /// </summary>
    public Guid AuthorId { get; set; }

    // Navigation Properties

    /// <summary>
    /// Reference to the task being commented on.
    /// Provides context for the discussion.
    /// </summary>
    public TaskItem Task { get; set; } = null!;

    /// <summary>
    /// Reference to the user who authored the comment.
    /// Allows displaying author name and avatar.
    /// </summary>
    public User Author { get; set; } = null!;
}