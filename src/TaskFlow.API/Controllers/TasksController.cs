using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Features.Tasks.Commands.CreateTask;
using TaskFlow.Application.Features.Tasks.Commands.DeleteTask;
using TaskFlow.Application.Features.Tasks.Commands.UpdateTask;
using TaskFlow.Application.Features.Tasks.Queries.GetTaskById;
using TaskFlow.Application.Features.Tasks.Queries.GetTasksByProject;

namespace TaskFlow.API.Controllers;

/// <summary>
/// Controller for task operations.
/// All endpoints require JWT authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets a task by its ID.
    /// User must have access to the project containing the task.
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <response code="200">Task found and returned</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">No permission to view this task</response>
    /// <response code="404">Task not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
        try
        {
            var query = new GetTaskByIdQuery { TaskId = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to task {TaskId}: {Message}", id, ex.Message);
            return Forbid();
        }
    }

    /// <summary>
    /// Gets all tasks for a specific project.
    /// User must have access to the project.
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <response code="200">Tasks found and returned</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">No permission to view this project's tasks</response>
    [HttpGet("project/{projectId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTasksByProject(Guid projectId)
    {
        try
        {
            var query = new GetTasksByProjectQuery { ProjectId = projectId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to project {ProjectId} tasks: {Message}",
                projectId, ex.Message);
            return Forbid();
        }
    }

    /// <summary>
    /// Creates a new task in a project.
    /// User must have access to the project.
    /// </summary>
    /// <param name="command">Task creation data</param>
    /// <response code="201">Task created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">No permission to create tasks in this project</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            _logger.LogInformation("Task created: {TaskId} in project {ProjectId}",
                result.Id, result.ProjectId);

            // Return 201 Created with location header
            return CreatedAtAction(
                nameof(GetTaskById),
                new { id = result.Id },
                result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized task creation attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid task creation data: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing task.
    /// User must have permission to modify the task.
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="command">Updated task data</param>
    /// <response code="200">Task updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">No permission to modify this task</response>
    /// <response code="404">Task not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskCommand command)
    {
        // Ensure the ID in the route matches the command
        command.TaskId = id;

        try
        {
            var result = await _mediator.Send(command);

            _logger.LogInformation("Task updated: {TaskId}", id);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized task update attempt on {TaskId}: {Message}",
                id, ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid task update data for {TaskId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a task.
    /// User must have permission to delete the task (project owner or task assignee).
    /// This will also delete all associated comments.
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <response code="204">Task deleted successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">No permission to delete this task</response>
    /// <response code="404">Task not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        try
        {
            var command = new DeleteTaskCommand { TaskId = id };
            await _mediator.Send(command);

            _logger.LogInformation("Task deleted: {TaskId}", id);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized task deletion attempt on {TaskId}: {Message}",
                id, ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}