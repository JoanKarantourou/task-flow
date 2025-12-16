using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Command to refresh an expired access token using a valid refresh token.
/// Returns new tokens (access + refresh) upon successful refresh.
/// Implements refresh token rotation for security.
/// </summary>
public class RefreshTokenCommand : IRequest<TokenDto>
{
    /// <summary>
    /// The expired access token.
    /// Used to extract user ID (even though expired).
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// The current refresh token.
    /// Must match the one stored in the database.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
