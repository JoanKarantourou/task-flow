using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Represents the many-to-many relationship between Users and Projects.
/// Tracks which users are members of which projects and their roles.
/// </summary>
public class ProjectMember : BaseEntity
{
    /// <summary>
    /// ID of the project this membership belongs to.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// ID of the user who is a member of the project.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Role of the user within the project.
    /// Common values: "Owner", "Admin", "Member"
    /// - Owner: Full control, can delete project
    /// - Admin: Can manage members and settings
    /// - Member: Can view and create tasks
    /// </summary>
    public string Role { get; set; } = "Member";

    // Navigation Properties

    /// <summary>
    /// Reference to the project.
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Reference to the user who is a member.
    /// </summary>
    public User User { get; set; } = null!;
}