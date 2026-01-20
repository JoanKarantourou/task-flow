using MediatR;

namespace TaskFlow.Application.Features.Comments.Commands.DeleteComment;

/// <summary>
/// Command to delete a comment.
/// Only the comment author or project owner can delete a comment.
/// </summary>
public class DeleteCommentCommand : IRequest<bool>
{
    /// <summary>
    /// ID of the comment to delete.
    /// </summary>
    public Guid CommentId { get; set; }
}
