using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Comments.Queries.GetCommentsByTask;

/// <summary>
/// Query to get all comments for a specific task.
/// User must have access to the project containing the task.
/// </summary>
public class GetCommentsByTaskQuery : IRequest<List<CommentDto>>
{
    /// <summary>
    /// ID of the task to get comments for.
    /// </summary>
    public Guid TaskId { get; set; }
}
