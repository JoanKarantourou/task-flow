using System.Xml.Linq;
using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Represents a user in the TaskFlow system.
/// Users can own projects, be assigned tasks, and collaborate with teams.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// User's email address - used for login and communication.
    /// Must be unique across the system.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's first name for display purposes.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name for display purposes.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password for authentication.
    /// Never store plain text passwords - always use BCrypt or similar.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Current refresh token for JWT authentication.
    /// Null when user is logged out.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Expiration time for the refresh token.
    /// After this time, user must log in again.
    /// </summary>
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Navigation Properties

    /// <summary>
    /// Collection of projects owned by this user.
    /// A user can own multiple projects.
    /// </summary>
    public ICollection<Project> Projects { get; set; } = new List<Project>();

    /// <summary>
    /// Collection of tasks assigned to this user.
    /// A user can have multiple tasks assigned across different projects.
    /// </summary>
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();

    /// <summary>
    /// Collection of project memberships.
    /// Represents all projects this user is a member of (not just owned).
    /// </summary>
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();

    /// <summary>
    /// Collection of comments authored by this user.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Computed property to get user's full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}