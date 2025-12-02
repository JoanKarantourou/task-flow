namespace TaskFlow.Application.DTOs;

/// <summary>
/// Data Transfer Object for User entity.
/// Used for API responses when returning user information.
/// Does NOT include sensitive data like password hash or refresh tokens.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Unique identifier of the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's full name (computed from first and last name).
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// When the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Simplified user DTO for lists and references.
/// Contains minimal information needed to identify and display a user.
/// Used in dropdowns, member lists, and assignee displays.
/// </summary>
public class UserSummaryDto
{
    /// <summary>
    /// Unique identifier of the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// DTO for user registration requests.
/// Contains the data needed to create a new user account.
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// Email address for the new account.
    /// Must be unique across the system.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password for the new account.
    /// Will be hashed before storage.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Password confirmation - must match Password.
    /// Used to prevent typos during registration.
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

/// <summary>
/// DTO for login requests.
/// Contains credentials for authentication.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Email address of the account.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Account password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO for authentication responses.
/// Contains JWT tokens for API authentication.
/// </summary>
public class TokenDto
{
    /// <summary>
    /// JWT access token for API requests.
    /// Short-lived (typically 15-60 minutes).
    /// Include in Authorization header: "Bearer {token}"
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token for obtaining new access tokens.
    /// Long-lived (typically 7-30 days).
    /// Used to get new access token when current one expires.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// When the access token expires (UTC).
    /// Client should refresh token before this time.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Information about the authenticated user.
    /// </summary>
    public UserDto User { get; set; } = null!;
}

/// <summary>
/// DTO for refresh token requests.
/// Used to obtain a new access token using a refresh token.
/// </summary>
public class RefreshTokenDto
{
    /// <summary>
    /// The expired or soon-to-expire access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// The refresh token received during login.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
