using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for the User entity.
/// Defines how the User entity maps to the database table and its relationships.
/// Implements IEntityTypeConfiguration to keep configuration separate from DbContext.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Configures the User entity mapping using Fluent API.
    /// This method is called automatically by EF Core when building the model.
    /// </summary>
    /// <param name="builder">Builder used to configure the User entity</param>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table Configuration
        // Explicitly name the table (optional, but good practice for clarity)
        builder.ToTable("Users");

        // Primary Key
        // HasKey defines which property is the primary key
        builder.HasKey(u => u.Id);

        // Property Configurations

        // Email: Required, unique, with max length
        builder.Property(u => u.Email)
            .IsRequired()                // Cannot be null (NOT NULL in SQL)
            .HasMaxLength(256);          // Limit email length to 256 characters

        // Create unique index on Email to prevent duplicate accounts
        // This enforces uniqueness at database level for data integrity
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // FirstName: Required field with reasonable max length
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        // LastName: Required field with reasonable max length
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // PasswordHash: Required field to store hashed password
        // We use 500 max length to accommodate various hashing algorithms
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        // RefreshToken: Optional field (nullable)
        // Stores JWT refresh token for token rotation
        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);          // Not required - null when user is logged out

        // RefreshTokenExpiryTime: Optional DateTime
        // Stores when the refresh token expires
        // No configuration needed - EF Core handles DateTime properly by default

        // Relationship Configurations

        // One-to-Many: User -> Projects (User owns many Projects)
        builder.HasMany(u => u.Projects)                    // User has many Projects
            .WithOne(p => p.Owner)                          // Each Project has one Owner
            .HasForeignKey(p => p.OwnerId)                  // Foreign key is OwnerId in Project table
            .OnDelete(DeleteBehavior.Restrict);             // Prevent deleting user if they own projects
                                                            // Restrict ensures data integrity

        // One-to-Many: User -> Tasks (User can be assigned many Tasks)
        builder.HasMany(u => u.AssignedTasks)               // User has many assigned Tasks
            .WithOne(t => t.Assignee)                       // Each Task has one Assignee
            .HasForeignKey(t => t.AssigneeId)               // Foreign key is AssigneeId in Task table
            .OnDelete(DeleteBehavior.SetNull);              // If user deleted, set AssigneeId to null
                                                            // SetNull keeps tasks but removes assignment

        // One-to-Many: User -> Comments (User can write many Comments)
        builder.HasMany(u => u.Comments)                    // User has many Comments
            .WithOne(c => c.Author)                         // Each Comment has one Author
            .HasForeignKey(c => c.AuthorId)                 // Foreign key is AuthorId in Comment table
            .OnDelete(DeleteBehavior.Cascade);              // If user deleted, delete their comments
                                                            // Cascade maintains referential integrity

        // One-to-Many: User -> ProjectMembers (User can be member of many Projects)
        builder.HasMany(u => u.ProjectMemberships)          // User has many ProjectMemberships
            .WithOne(pm => pm.User)                         // Each ProjectMember has one User
            .HasForeignKey(pm => pm.UserId)                 // Foreign key is UserId in ProjectMember table
            .OnDelete(DeleteBehavior.Cascade);              // If user deleted, remove their memberships

        // Ignore computed properties (not stored in database)
        // FullName is calculated from FirstName + LastName, so we don't store it
        builder.Ignore(u => u.FullName);
    }
}