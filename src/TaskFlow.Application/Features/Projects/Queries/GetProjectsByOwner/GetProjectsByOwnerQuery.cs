using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Projects.Queries.GetProjectsByOwner;

/// <summary>
/// Query to get all projects that the current user owns or is a member of.
/// Automatically uses the current authenticated user.
/// </summary>
public class GetProjectsByOwnerQuery : IRequest<List<ProjectDto>>
{
    // No parameters needed - uses current user from JWT token
}