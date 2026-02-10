using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Dashboard.Queries.GetDashboardStats;

/// <summary>
/// Query to get aggregated dashboard statistics.
/// Returns project and task counts for the current user.
/// </summary>
public class GetDashboardStatsQuery : IRequest<DashboardStatsDto>
{
}
