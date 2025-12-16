using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new user account.
/// Returns tokens (access + refresh) and user information upon successful registration.
/// </summary>
public class RegisterCommand : IRequest<TokenDto>
{
    /// <summary>
    /// User's email address (must be unique).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password (will be hashed before storage).
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Password confirmation (must match Password).
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// User's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}
