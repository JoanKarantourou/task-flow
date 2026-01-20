using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Comments.Commands.UpdateComment;

/// <summary>
/// Command to update an existing comment.
/// Only the comment author can update their comment.
/// </summary>
public class UpdateCommentCommand : IRequest<CommentDto>
{
    /// <summary>
    /// ID of the comment to update.
    /// </summary>
    public Guid CommentId { get; set; }

    /// <summary>
    /// Updated comment content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
