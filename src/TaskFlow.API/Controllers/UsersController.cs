using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Features.Users.Commands.UpdateProfile;
using TaskFlow.Application.Features.Users.Queries.GetCurrentUser;

namespace TaskFlow.API.Controllers;

/// <summary>
/// Controller for user profile operations.
/// All endpoints require JWT authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current authenticated user's profile.
    /// </summary>
    /// <response code="200">User profile retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var query = new GetCurrentUserQuery();
            var result = await _mediator.Send(query);

            _logger.LogInformation("User profile retrieved for {UserId}", result.Id);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized profile access: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the current authenticated user's profile.
    /// </summary>
    /// <param name="command">Updated profile data</param>
    /// <response code="200">Profile updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Not authenticated</response>
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            _logger.LogInformation("User profile updated for {UserId}", result.Id);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized profile update: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Profile update failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }
}
