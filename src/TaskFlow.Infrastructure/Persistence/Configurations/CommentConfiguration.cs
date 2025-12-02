using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for the Comment entity.
/// Defines how the Comment entity maps to the database table and its relationships.
/// </summary>
public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    /// <summary>
    /// Configures the Comment entity mapping using Fluent API.
    /// </summary>
    /// <param name="builder">Builder used to configure the Comment entity</param>
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        // Table Configuration
        builder.ToTable("Comments");

        // Primary Key
        builder.HasKey(c => c.Id);

        // Property Configurations

        // Content: Required text content of the comment
        // Allow longer text for detailed comments and discussions
        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(2000);

        // TaskId: Required - every comment must belong to a task
        builder.Property(c => c.TaskId)
            .IsRequired();

        // AuthorId: Required - must know who wrote the comment
        builder.Property(c => c.AuthorId)
            .IsRequired();

        // Indexes for Query Performance

        // Index on TaskId - frequently query "all comments on a task"
        // This is the most common query pattern for comments
        builder.HasIndex(c => c.TaskId);

        // Index on AuthorId - useful for "all comments by user"
        builder.HasIndex(c => c.AuthorId);

        // Composite index for ordered comments: task and creation date
        // Optimizes queries like "show comments for task X ordered by time"
        builder.HasIndex(c => new { c.TaskId, c.CreatedAt });

        // Relationship Configurations

        // Many-to-One: Comments -> Task (Many comments belong to one Task)
        builder.HasOne(c => c.Task)                         // Comment belongs to one Task
            .WithMany(t => t.Comments)                      // Task has many Comments
            .HasForeignKey(c => c.TaskId)                   // Foreign key is TaskId
            .OnDelete(DeleteBehavior.Cascade);              // If task deleted, delete all its comments
                                                            // Cascade ensures no orphaned comments

        // Many-to-One: Comments -> User (Many comments written by one Author)
        builder.HasOne(c => c.Author)                       // Comment has one Author
            .WithMany(u => u.Comments)                      // User (Author) has many Comments
            .HasForeignKey(c => c.AuthorId)                 // Foreign key is AuthorId
            .OnDelete(DeleteBehavior.Cascade);              // If user deleted, delete their comments
                                                            // Cascade maintains data integrity
    }
}