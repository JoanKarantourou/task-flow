using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Features.Comments.Commands.CreateComment;
using TaskFlow.Application.Features.Comments.Commands.DeleteComment;
using TaskFlow.Application.Features.Comments.Commands.UpdateComment;
using TaskFlow.Application.Features.Comments.Queries.GetCommentsByTask;

namespace TaskFlow.API.Controllers;

/// <summary>
/// Controller for comment operations on tasks.
/// All endpoints require JWT authentication.
/// </summary>
[ApiController]
[Route("api/tasks/{taskId}/[controller]")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(IMediator mediator, ILogger<CommentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all comments for a specific task.
    /// User must have access to the project containing the task.
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <response code="200">Comments retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">No permission to view comments on this task</response>
    /// <response code="404">Task not found</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommentsByTask(Guid taskId)
    {
        try
        {
            var query = new GetCommentsByTaskQuery { TaskId = taskId };
            var result = await _mediator.Send(query);

            _logger.LogInformation("Retrieved {Count} comments for task {TaskId}", result.Count, taskId);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to task {TaskId} comments: {Message}", taskId, ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new comment on a task.
    /// User must have access to the project containing the task.
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="command">Comment creation data</param>
    /// <response code="201">Comment created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">No permission to comment on this task</response>
    /// <response code="404">Task not found</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateComment(Guid taskId, [FromBody] CreateCommentCommand command)
    {
        // Ensure the task ID matches
        command.TaskId = taskId;

        try
        {
            var result = await _mediator.Send(command);

            _logger.LogInformation("Comment created: {CommentId} on task {TaskId}", result.Id, taskId);

            return CreatedAtAction(
                nameof(GetCommentsByTask),
                new { taskId },
                result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized comment creation on task {TaskId}: {Message}", taskId, ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing comment.
    /// Only the comment author can update their comment.
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="commentId">Comment ID</param>
    /// <param name="command">Updated comment data</param>
    /// <response code="200">Comment updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">No permission to update this comment</response>
    /// <response code="404">Comment not found</response>
    [HttpPut("{commentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateComment(Guid taskId, Guid commentId, [FromBody] UpdateCommentCommand command)
    {
        // Ensure the comment ID matches
        command.CommentId = commentId;

        try
        {
            var result = await _mediator.Send(command);

            _logger.LogInformation("Comment updated: {CommentId} on task {TaskId}", commentId, taskId);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized comment update {CommentId}: {Message}", commentId, ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a comment.
    /// Only the comment author or project owner can delete a comment.
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="commentId">Comment ID</param>
    /// <response code="204">Comment deleted successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">No permission to delete this comment</response>
    /// <response code="404">Comment not found</response>
    [HttpDelete("{commentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(Guid taskId, Guid commentId)
    {
        try
        {
            var command = new DeleteCommentCommand { CommentId = commentId };
            await _mediator.Send(command);

            _logger.LogInformation("Comment deleted: {CommentId} from task {TaskId}", commentId, taskId);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized comment deletion {CommentId}: {Message}", commentId, ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
