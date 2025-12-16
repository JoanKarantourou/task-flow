namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Service interface for accessing the current authenticated user's information.
/// Extracts user details from JWT claims in the HTTP context.
/// </summary>
/// <remarks>
/// This service is used by handlers and controllers to identify who is making the request.
/// It reads claims from the JWT token that was validated by the authentication middleware.
/// 
/// Typical usage:
/// - Check if user is authenticated
/// - Get user ID for authorization checks
/// - Get user info for audit logging
/// </remarks>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user.
    /// Returns null if no user is authenticated (anonymous request).
    /// </summary>
    /// <example>
    /// var userId = _currentUserService.UserId;
    /// if (userId == null)
    /// {
    ///     throw new UnauthorizedAccessException("User must be logged in");
    /// }
    /// 
    /// // Use userId to filter data
    /// var myProjects = await _unitOfWork.Projects.GetProjectsByOwnerAsync(userId.Value);
    /// </example>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the email address of the currently authenticated user.
    /// Returns null if no user is authenticated.
    /// </summary>
    /// <example>
    /// var email = _currentUserService.Email;
    /// _logger.LogInformation($"User {email} accessed this resource");
    /// </example>
    string? Email { get; }

    /// <summary>
    /// Gets the full name of the currently authenticated user.
    /// Returns null if no user is authenticated.
    /// </summary>
    /// <example>
    /// var name = _currentUserService.FullName;
    /// var notification = $"Welcome back, {name}!";
    /// </example>
    string? FullName { get; }

    /// <summary>
    /// Checks if a user is currently authenticated.
    /// Returns true if UserId is not null.
    /// </summary>
    /// <example>
    /// if (!_currentUserService.IsAuthenticated)
    /// {
    ///     return Unauthorized("You must be logged in");
    /// }
    /// </example>
    bool IsAuthenticated { get; }
}