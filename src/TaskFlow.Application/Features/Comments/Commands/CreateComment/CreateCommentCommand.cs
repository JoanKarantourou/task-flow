using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Comments.Commands.CreateComment;

/// <summary>
/// Command to create a new comment on a task.
/// User must have access to the project containing the task.
/// </summary>
public class CreateCommentCommand : IRequest<CommentDto>
{
    /// <summary>
    /// ID of the task to comment on.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// The comment text content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
