using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Service interface for JWT token generation and validation.
/// Handles creating access tokens and refresh tokens for authentication.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for a user.
    /// The token contains user claims (Id, Email, Name) and expires after a short time (typically 15-60 minutes).
    /// </summary>
    /// <param name="user">The user to generate the token for</param>
    /// <returns>JWT token string</returns>
    /// <example>
    /// var token = _tokenService.GenerateAccessToken(user);
    /// // Returns: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// </example>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token for obtaining new access tokens.
    /// This is a cryptographically secure random string.
    /// Refresh tokens are long-lived (typically 7-30 days) and stored in the database.
    /// </summary>
    /// <returns>Random refresh token string</returns>
    /// <example>
    /// var refreshToken = _tokenService.GenerateRefreshToken();
    /// // Returns: "Xy7k2m9p4n8q1w5e3r6t0y8u4i2o7p5a9s1d3f6g8h0j2k4l6"
    /// </example>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT token and extracts the user ID from its claims.
    /// Returns null if the token is invalid, expired, or malformed.
    /// </summary>
    /// <param name="token">The JWT token to validate</param>
    /// <returns>User ID if valid, null otherwise</returns>
    /// <example>
    /// var userId = _tokenService.ValidateToken(expiredToken);
    /// if (userId == null)
    /// {
    ///     // Token is invalid or expired
    /// }
    /// </example>
    Guid? ValidateToken(string token);

    /// <summary>
    /// Gets the token expiration time in minutes.
    /// Used to inform the client when to refresh the token.
    /// </summary>
    /// <returns>Number of minutes until token expires</returns>
    int GetTokenExpirationMinutes();
}

// ============================================
// JWT Token Explained
// ============================================
//
// A JWT (JSON Web Token) has 3 parts separated by dots:
// Header.Payload.Signature
//
// Example token:
// eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
//
// Part 1 - Header (Base64 encoded):
// {
//   "alg": "HS256",      // Algorithm used
//   "typ": "JWT"         // Token type
// }
//
// Part 2 - Payload (Base64 encoded):
// {
//   "sub": "user-id",              // Subject (user ID)
//   "email": "user@example.com",   // User email
//   "name": "John Doe",            // User name
//   "exp": 1516239022              // Expiration timestamp
// }
//
// Part 3 - Signature:
// HMACSHA256(
//   base64UrlEncode(header) + "." + base64UrlEncode(payload),
//   secret
// )
//
// ============================================
// Access Token vs Refresh Token
// ============================================
//
// Access Token (JWT):
// - Short-lived (15-60 minutes)
// - Contains user info and claims
// - Sent with every API request
// - Cannot be revoked once issued
// - Stored in memory or localStorage
//
// Refresh Token:
// - Long-lived (7-30 days)
// - Random string, no user info
// - Used to get new access tokens
// - Can be revoked (stored in database)
// - Stored securely (httpOnly cookie or secure storage)
//
// Flow:
// 1. User logs in → Receives both tokens
// 2. Use access token for API calls
// 3. Access token expires → Use refresh token to get new access token
// 4. Refresh token expires → User must log in again
//
// ============================================
// Security Notes
// ============================================
//
// Why short-lived access tokens?
// - If stolen, attacker only has 15-60 minutes
// - Limits damage from token theft
// - Forces periodic re-authentication
//
// Why refresh tokens?
// - Better user experience (don't login every hour)
// - Can be revoked if compromised
// - Server can track and invalidate them
//
// Token Rotation:
// - When refresh token is used, issue a NEW refresh token
// - Invalidate the old one
// - Prevents refresh token reuse attacks
