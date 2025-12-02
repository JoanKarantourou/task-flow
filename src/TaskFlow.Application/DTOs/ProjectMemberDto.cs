namespace TaskFlow.Application.DTOs;

/// <summary>
/// Data Transfer Object for ProjectMember entity.
/// Represents a user's membership in a project with their role.
/// Used for displaying team members and managing project access.
/// </summary>
public class ProjectMemberDto
{
    /// <summary>
    /// Unique identifier of the membership record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the project.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// ID of the user who is a member.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Full name of the member.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Email of the member.
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Role of the user in the project.
    /// Common values: "Owner", "Admin", "Member"
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// When the user was added to the project.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for adding a member to a project.
/// </summary>
public class AddProjectMemberDto
{
    /// <summary>
    /// ID of the user to add to the project.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Role to assign to the user.
    /// Defaults to "Member" if not specified.
    /// </summary>
    public string Role { get; set; } = "Member";
}

/// <summary>
/// DTO for updating a project member's role.
/// </summary>
public class UpdateProjectMemberDto
{
    /// <summary>
    /// The new role for the member.
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
