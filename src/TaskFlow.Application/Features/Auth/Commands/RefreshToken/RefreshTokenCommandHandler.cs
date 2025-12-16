using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Handler for RefreshTokenCommand.
/// Uses AuthService to refresh expired tokens.
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenDto>
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Constructor - DI injects dependencies.
    /// </summary>
    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Handles the refresh token command.
    /// Generates new tokens using the refresh token.
    /// </summary>
    public async Task<TokenDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Create RefreshTokenDto from command
        var refreshTokenDto = new RefreshTokenDto
        {
            AccessToken = request.AccessToken,
            RefreshToken = request.RefreshToken
        };

        // Call AuthService to refresh tokens
        // This will:
        // 1. Extract user ID from expired access token
        // 2. Find user in database
        // 3. Validate refresh token matches stored token
        // 4. Check refresh token hasn't expired
        // 5. Generate NEW access token
        // 6. Generate NEW refresh token (rotation!)
        // 7. Invalidate OLD refresh token
        // 8. Return TokenDto with new tokens
        var tokenDto = await _authService.RefreshTokenAsync(refreshTokenDto, cancellationToken);

        return tokenDto;
    }
}

// ============================================
// Token Refresh Flow
// ============================================
//
// Client detects access token expired
//     ↓
// Controller → MediatR.Send(RefreshTokenCommand)
//     ↓
// ValidationBehavior validates command
//     ↓
// RefreshTokenCommandHandler.Handle()
//     ↓
// AuthService.RefreshTokenAsync()
//     ↓
// - Extract user ID from expired access token
// - Find user in database
// - Validate refresh token matches
// - Check refresh token not expired
// - Generate NEW access token
// - Generate NEW refresh token
// - Update user with NEW refresh token
// - Invalidate OLD refresh token
//     ↓
// Return TokenDto { NewAccessToken, NewRefreshToken, User }
//     ↓
// Controller returns 200 OK with new tokens
//     ↓
// Client stores new tokens
//
// ============================================
// Refresh Token Rotation Explained
// ============================================
//
// Why rotate refresh tokens?
// - Security: Prevents token reuse
// - Detection: If old token used again, it's stolen
// - Mitigation: Can invalidate all tokens for user
//
// Flow:
// 1. User has: AccessToken1, RefreshToken1
// 2. AccessToken1 expires
// 3. User sends RefreshToken1
// 4. Server issues: AccessToken2, RefreshToken2
// 5. Server invalidates RefreshToken1
// 6. User now has: AccessToken2, RefreshToken2
//
// If attacker steals RefreshToken1:
// - User refreshes first → RefreshToken1 invalidated
// - Attacker tries RefreshToken1 → Rejected (invalid)
// - Or attacker refreshes first → User's refresh fails
//   → Triggers security alert → Invalidate all tokens
//
// ============================================
// Error Handling
// ============================================
//
// AuthService throws:
// - UnauthorizedAccessException: "Invalid access token"
//   → Access token malformed or invalid
//
// - UnauthorizedAccessException: "User not found"
//   → User ID from token doesn't exist
//
// - UnauthorizedAccessException: "Invalid refresh token"
//   → Refresh token doesn't match stored token
//   → Could indicate token theft!
//
// - UnauthorizedAccessException: "Refresh token has expired"
//   → User must log in again
//
// All caught by middleware → Returns 401 Unauthorized
//
// ============================================
// Client Implementation
// ============================================
//
// Client-side token management:
//
// 1. Store tokens after login/register:
//    localStorage.setItem('accessToken', tokens.accessToken);
//    localStorage.setItem('refreshToken', tokens.refreshToken);
//
// 2. Add access token to API requests:
//    headers: { Authorization: `Bearer ${accessToken}` }
//
// 3. Handle 401 responses:
//    if (response.status === 401) {
//      // Try to refresh
//      const newTokens = await refreshTokens();
//      // Retry original request with new token
//    }
//
// 4. Refresh tokens:
//    const newTokens = await api.post('/auth/refresh', {
//      accessToken: oldAccessToken,
//      refreshToken: oldRefreshToken
//    });
//    // Store new tokens
//    // Retry failed request
//
// 5. Handle refresh failure:
//    if (refreshFails) {
//      // Clear tokens
//      // Redirect to login
//    }
