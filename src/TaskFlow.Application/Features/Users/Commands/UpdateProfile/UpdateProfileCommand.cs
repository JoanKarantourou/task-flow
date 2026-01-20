using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Users.Commands.UpdateProfile;

/// <summary>
/// Command to update the current user's profile.
/// </summary>
public class UpdateProfileCommand : IRequest<UserDto>
{
    /// <summary>
    /// Updated first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Updated last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Updated email address.
    /// </summary>
    public string? Email { get; set; }
}
