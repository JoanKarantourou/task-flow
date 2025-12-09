using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Features.Auth.Commands.Login;
using TaskFlow.Application.Features.Auth.Commands.RefreshToken;
using TaskFlow.Application.Features.Auth.Commands.Register;

namespace TaskFlow.API.Controllers;

/// <summary>
/// Controller for authentication operations.
/// Handles user registration, login, and token refresh.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Constructor - DI injects dependencies.
    /// </summary>
    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="command">Registration information</param>
    /// <returns>Access token, refresh token, and user information</returns>
    /// <response code="200">Registration successful, returns tokens and user info</response>
    /// <response code="400">Invalid input data or passwords don't match</response>
    /// <response code="409">Email already registered</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}", command.Email);

            // Send command through MediatR pipeline
            // This will:
            // 1. Validate input via ValidationBehavior
            // 2. Log via LoggingBehavior
            // 3. Execute RegisterCommandHandler
            // 4. Call AuthService.RegisterAsync()
            var result = await _mediator.Send(command);

            _logger.LogInformation("User registered successfully: {Email}", command.Email);

            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already registered"))
        {
            // Email already exists
            _logger.LogWarning("Registration failed - Email already exists: {Email}", command.Email);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // Passwords don't match or other validation error
            _logger.LogWarning("Registration failed - Validation error: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Authenticates a user and generates tokens.
    /// </summary>
    /// <param name="command">Login credentials</param>
    /// <returns>Access token, refresh token, and user information</returns>
    /// <response code="200">Login successful, returns tokens and user info</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", command.Email);

            // Send command through MediatR pipeline
            var result = await _mediator.Send(command);

            _logger.LogInformation("User logged in successfully: {Email}", command.Email);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Invalid credentials (wrong email or password)
            _logger.LogWarning("Login failed for email: {Email}", command.Email);
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Refreshes an expired access token using a refresh token.
    /// Returns new access token and new refresh token (token rotation).
    /// </summary>
    /// <param name="command">Expired access token and current refresh token</param>
    /// <returns>New access token, new refresh token, and user information</returns>
    /// <response code="200">Token refresh successful, returns new tokens</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Invalid or expired refresh token</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        try
        {
            _logger.LogInformation("Token refresh attempt");

            // Send command through MediatR pipeline
            var result = await _mediator.Send(command);

            _logger.LogInformation("Token refreshed successfully");

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Invalid or expired refresh token
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
    }
}