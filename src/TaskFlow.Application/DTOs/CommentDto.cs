namespace TaskFlow.Application.DTOs;

/// <summary>
/// Data Transfer Object for Comment entity.
/// Used for API responses when returning comment information.
/// Contains comment text and author details.
/// </summary>
public class CommentDto
{
    /// <summary>
    /// Unique identifier of the comment.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The text content of the comment.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// ID of the task this comment belongs to.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// ID of the user who wrote this comment.
    /// </summary>
    public Guid AuthorId { get; set; }

    /// <summary>
    /// Full name of the comment author.
    /// Included to avoid additional query for display.
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Email of the comment author.
    /// Useful for displaying avatar or contacting author.
    /// </summary>
    public string AuthorEmail { get; set; } = string.Empty;

    /// <summary>
    /// When the comment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the comment was last updated.
    /// Helps identify edited comments in UI.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Whether the comment has been edited.
    /// True if UpdatedAt is different from CreatedAt.
    /// </summary>
    public bool IsEdited => UpdatedAt > CreatedAt.AddSeconds(1);
}

/// <summary>
/// DTO for creating a new comment.
/// Contains only the data needed to create a comment.
/// </summary>
public class CreateCommentDto
{
    /// <summary>
    /// ID of the task to comment on.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// The comment text.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating an existing comment.
/// Contains only the fields that can be updated.
/// </summary>
public class UpdateCommentDto
{
    /// <summary>
    /// The updated comment text.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
