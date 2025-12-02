using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for the Project entity.
/// Defines how the Project entity maps to the database table and its relationships.
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    /// <summary>
    /// Configures the Project entity mapping using Fluent API.
    /// </summary>
    /// <param name="builder">Builder used to configure the Project entity</param>
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        // Table Configuration
        builder.ToTable("Projects");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Property Configurations

        // Name: Required project name with reasonable max length
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Description: Optional detailed description
        // Using HasMaxLength(2000) to allow detailed project descriptions
        builder.Property(p => p.Description)
            .HasMaxLength(2000);             // Not required - can be null

        // Status: Enum stored as integer in database
        // EF Core automatically converts enum to int
        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();           // Store enum as integer (0, 1, 2, 3)

        // OwnerId: Required foreign key to User
        builder.Property(p => p.OwnerId)
            .IsRequired();

        // StartDate: Optional - some projects may not have defined start date
        // No configuration needed - EF Core handles nullable DateTime

        // DueDate: Optional - not all projects have deadlines
        // No configuration needed - EF Core handles nullable DateTime

        // Indexes for Query Performance

        // Index on OwnerId for faster queries like "get all projects by owner"
        builder.HasIndex(p => p.OwnerId);

        // Index on Status for filtering projects by status
        builder.HasIndex(p => p.Status);

        // Composite index for common query: projects by owner and status
        // This optimizes queries like "get active projects for user X"
        builder.HasIndex(p => new { p.OwnerId, p.Status });

        // Relationship Configurations

        // Many-to-One: Projects -> User (Many projects belong to one Owner)
        // This is the inverse of User -> Projects relationship
        builder.HasOne(p => p.Owner)                        // Project has one Owner
            .WithMany(u => u.Projects)                      // User (Owner) has many Projects
            .HasForeignKey(p => p.OwnerId)                  // Foreign key is OwnerId
            .OnDelete(DeleteBehavior.Restrict);             // Cannot delete user if they own projects
                                                            // Business rule: reassign or delete projects first

        // One-to-Many: Project -> Tasks (Project contains many Tasks)
        builder.HasMany(p => p.Tasks)                       // Project has many Tasks
            .WithOne(t => t.Project)                        // Each Task belongs to one Project
            .HasForeignKey(t => t.ProjectId)                // Foreign key is ProjectId in Task table
            .OnDelete(DeleteBehavior.Cascade);              // If project deleted, delete all its tasks
                                                            // Cascade ensures cleanup of orphaned tasks

        // One-to-Many: Project -> ProjectMembers (Project has many Members)
        builder.HasMany(p => p.Members)                     // Project has many Members
            .WithOne(pm => pm.Project)                      // Each ProjectMember belongs to one Project
            .HasForeignKey(pm => pm.ProjectId)              // Foreign key is ProjectId
            .OnDelete(DeleteBehavior.Cascade);              // If project deleted, remove all memberships
    }
}