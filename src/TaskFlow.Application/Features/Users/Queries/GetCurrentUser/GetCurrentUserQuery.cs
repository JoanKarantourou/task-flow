using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Users.Queries.GetCurrentUser;

/// <summary>
/// Query to get the current authenticated user's profile.
/// </summary>
public class GetCurrentUserQuery : IRequest<UserDto>
{
}
