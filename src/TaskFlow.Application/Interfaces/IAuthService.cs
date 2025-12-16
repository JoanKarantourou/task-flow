using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Service interface for authentication operations.
/// Handles user registration, login, and token refresh.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account.
    /// Validates that email is unique, hashes the password, and creates the user.
    /// </summary>
    /// <param name="registerDto">Registration information (email, password, name)</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>TokenDto containing access token, refresh token, and user info</returns>
    /// <exception cref="InvalidOperationException">Thrown when email already exists</exception>
    /// <exception cref="ArgumentException">Thrown when passwords don't match</exception>
    /// <example>
    /// var tokens = await _authService.RegisterAsync(new RegisterDto
    /// {
    ///     Email = "john@example.com",
    ///     Password = "SecurePass123!",
    ///     ConfirmPassword = "SecurePass123!",
    ///     FirstName = "John",
    ///     LastName = "Doe"
    /// });
    /// // Returns: TokenDto with access token, refresh token, and user info
    /// </example>
    Task<TokenDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user with email and password.
    /// Validates credentials and returns tokens if successful.
    /// </summary>
    /// <param name="loginDto">Login credentials (email and password)</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>TokenDto containing access token, refresh token, and user info</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid</exception>
    /// <example>
    /// var tokens = await _authService.LoginAsync(new LoginDto
    /// {
    ///     Email = "john@example.com",
    ///     Password = "SecurePass123!"
    /// });
    /// // Returns: TokenDto with fresh tokens
    /// </example>
    Task<TokenDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// Implements refresh token rotation: invalidates old refresh token and issues new one.
    /// </summary>
    /// <param name="refreshTokenDto">Contains the expired access token and refresh token</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>TokenDto with new access token and new refresh token</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when refresh token is invalid or expired</exception>
    /// <example>
    /// var newTokens = await _authService.RefreshTokenAsync(new RefreshTokenDto
    /// {
    ///     AccessToken = "expired-jwt-token",
    ///     RefreshToken = "current-refresh-token"
    /// });
    /// // Returns: TokenDto with NEW access token and NEW refresh token
    /// // Old refresh token is invalidated
    /// </example>
    Task<TokenDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken = default);
}

// ============================================
// Authentication Flow
// ============================================
//
// Registration Flow:
// 1. User submits registration form
// 2. Validate email is unique
// 3. Validate passwords match
// 4. Hash password with BCrypt
// 5. Create user in database
// 6. Generate access token + refresh token
// 7. Return tokens to client
// 8. Client stores tokens
//
// Login Flow:
// 1. User submits email + password
// 2. Find user by email
// 3. Verify password hash with BCrypt
// 4. Generate new access token + refresh token
// 5. Store refresh token in database (with expiry)
// 6. Return tokens to client
//
// Refresh Token Flow:
// 1. Client's access token expires
// 2. Client sends refresh token
// 3. Validate refresh token exists in database
// 4. Check refresh token hasn't expired
// 5. Generate NEW access token
// 6. Generate NEW refresh token (rotation)
// 7. Invalidate OLD refresh token
// 8. Store NEW refresh token in database
// 9. Return new tokens to client
//
// ============================================
// Security Best Practices
// ============================================
//
// Password Hashing:
// - Never store plain text passwords
// - Use BCrypt (or Argon2, PBKDF2)
// - BCrypt is slow by design (prevents brute force)
// - Each password has unique salt
//
// Refresh Token Security:
// - Store in database (can be revoked)
// - Set expiration date
// - Rotate on each use (issue new, invalidate old)
// - Track usage to detect theft
//
// Token Storage (Client-side):
// - Access token: Memory or localStorage
// - Refresh token: httpOnly cookie (best) or secure storage
// - Never expose tokens in URLs
// - Use HTTPS in production
//
// Rate Limiting:
// - Limit login attempts (prevent brute force)
// - Lock account after X failed attempts
// - Add CAPTCHA after Y failures
//
// ============================================
// Error Handling
// ============================================
//
// Registration Errors:
// - Email already exists → "Email already registered"
// - Passwords don't match → "Passwords do not match"
// - Weak password → "Password must meet requirements"
//
// Login Errors:
// - Wrong email → "Invalid credentials" (don't reveal which)
// - Wrong password → "Invalid credentials" (don't reveal which)
// - Account locked → "Account is locked. Try again later"
//
// Refresh Token Errors:
// - Token not found → "Invalid refresh token"
// - Token expired → "Refresh token expired. Please log in again"
// - Token already used → "Refresh token already used (possible theft)"
//
// Security Note: Don't reveal whether email exists!
// Always return generic "Invalid credentials" for login failures.
// This prevents attackers from enumerating valid email addresses.
