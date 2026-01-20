using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Features.Projects.Commands.CreateProject;
using TaskFlow.Application.Features.Projects.Commands.DeleteProject;
using TaskFlow.Application.Features.Projects.Commands.UpdateProject;
using TaskFlow.Application.Features.Projects.Queries.GetProjectById;
using TaskFlow.Application.Features.Projects.Queries.GetProjectsByOwner;

namespace TaskFlow.API.Controllers;

/// <summary>
/// Controller for project operations.
/// All endpoints require JWT authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IMediator mediator, ILogger<ProjectsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all projects the current user owns or is a member of.
    /// Automatically uses the authenticated user from JWT token.
    /// </summary>
    /// <response code="200">Projects found and returned</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyProjects()
    {
        try
        {
            var query = new GetProjectsByOwnerQuery();
            var result = await _mediator.Send(query);

            _logger.LogInformation("Retrieved {Count} projects for current user", result.Count);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a project by its ID.
    /// User must have access to the project (owner or member).
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <response code="200">Project found and returned</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">No permission to view this project</response>
    /// <response code="404">Project not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectById(Guid id)
    {
        try
        {
            var query = new GetProjectByIdQuery { ProjectId = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { message = $"Project with ID {id} not found" });
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to project {ProjectId}: {Message}", id, ex.Message);
            return Forbid();
        }
    }

    /// <summary>
    /// Creates a new project.
    /// Current user automatically becomes the project owner.
    /// </summary>
    /// <param name="command">Project creation data</param>
    /// <response code="201">Project created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            _logger.LogInformation("Project created: {ProjectId} by user {UserId}",
                result.Id, result.OwnerId);

            // Return 201 Created with location header
            return CreatedAtAction(
                nameof(GetProjectById),
                new { id = result.Id },
                result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized project creation attempt: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid project creation data: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing project.
    /// User must be the project owner.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="command">Updated project data</param>
    /// <response code="200">Project updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">No permission to update this project</response>
    /// <response code="404">Project not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectCommand command)
    {
        // Ensure the ID in the route matches the command
        command.ProjectId = id;

        try
        {
            var result = await _mediator.Send(command);

            _logger.LogInformation("Project updated: {ProjectId}", id);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized project update attempt on {ProjectId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid project update data for {ProjectId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a project.
    /// User must be the project owner.
    /// This will also delete all associated tasks and comments.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <response code="204">Project deleted successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">No permission to delete this project</response>
    /// <response code="404">Project not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        try
        {
            var command = new DeleteProjectCommand { ProjectId = id };
            await _mediator.Send(command);

            _logger.LogInformation("Project deleted: {ProjectId}", id);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized project deletion attempt on {ProjectId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}