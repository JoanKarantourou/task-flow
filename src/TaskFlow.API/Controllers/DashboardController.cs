using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Features.Dashboard.Queries.GetDashboardStats;

namespace TaskFlow.API.Controllers;

/// <summary>
/// Controller for dashboard operations.
/// Provides aggregated statistics for the dashboard view.
/// All endpoints require JWT authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets aggregated dashboard statistics for the current user.
    /// Includes project counts, task counts by status/priority, and overdue tasks.
    /// </summary>
    /// <response code="200">Statistics returned successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var query = new GetDashboardStatsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to dashboard stats: {Message}", ex.Message);
            return Forbid();
        }
    }
}
