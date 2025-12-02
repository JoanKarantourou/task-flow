using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for the ProjectMember entity.
/// This is a join/junction table for the many-to-many relationship between Users and Projects.
/// It includes an additional Role property to track member permissions.
/// </summary>
public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    /// <summary>
    /// Configures the ProjectMember entity mapping using Fluent API.
    /// </summary>
    /// <param name="builder">Builder used to configure the ProjectMember entity</param>
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        // Table Configuration
        builder.ToTable("ProjectMembers");

        // Primary Key
        // Even though this is a junction table, we use a single Id as primary key
        // This is simpler than composite keys and allows easier tracking
        builder.HasKey(pm => pm.Id);

        // Property Configurations

        // ProjectId: Required - must know which project
        builder.Property(pm => pm.ProjectId)
            .IsRequired();

        // UserId: Required - must know which user
        builder.Property(pm => pm.UserId)
            .IsRequired();

        // Role: Required - defines member's permissions in the project
        // Common values: "Owner", "Admin", "Member"
        builder.Property(pm => pm.Role)
            .IsRequired()
            .HasMaxLength(50)               // Reasonable length for role names
            .HasDefaultValue("Member");     // Default role when not specified

        // Indexes for Query Performance

        // Index on ProjectId - query "all members of a project"
        builder.HasIndex(pm => pm.ProjectId);

        // Index on UserId - query "all projects a user is member of"
        builder.HasIndex(pm => pm.UserId);

        // Unique composite index to prevent duplicate memberships
        // A user can only be a member of a project once
        // This enforces the business rule at database level
        builder.HasIndex(pm => new { pm.ProjectId, pm.UserId })
            .IsUnique();

        // Relationship Configurations

        // Many-to-One: ProjectMembers -> Project
        // Many memberships belong to one Project
        builder.HasOne(pm => pm.Project)                    // ProjectMember belongs to one Project
            .WithMany(p => p.Members)                       // Project has many Members
            .HasForeignKey(pm => pm.ProjectId)              // Foreign key is ProjectId
            .OnDelete(DeleteBehavior.Cascade);              // If project deleted, remove all memberships
                                                            // Cascade cleanup for junction table

        // Many-to-One: ProjectMembers -> User
        // Many memberships belong to one User
        builder.HasOne(pm => pm.User)                       // ProjectMember belongs to one User
            .WithMany(u => u.ProjectMemberships)            // User has many ProjectMemberships
            .HasForeignKey(pm => pm.UserId)                 // Foreign key is UserId
            .OnDelete(DeleteBehavior.Cascade);              // If user deleted, remove their memberships
                                                            // Cascade maintains referential integrity
    }
}