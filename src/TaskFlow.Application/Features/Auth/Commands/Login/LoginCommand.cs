using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user and generate tokens.
/// Returns tokens (access + refresh) and user information upon successful login.
/// </summary>
public class LoginCommand : IRequest<TokenDto>
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
