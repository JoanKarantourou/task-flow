using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for the TaskItem entity.
/// Defines how the TaskItem entity maps to the database table and its relationships.
/// Note: We map this to "Tasks" table, not "TaskItems", for cleaner SQL.
/// </summary>
public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    /// <summary>
    /// Configures the TaskItem entity mapping using Fluent API.
    /// </summary>
    /// <param name="builder">Builder used to configure the TaskItem entity</param>
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        // Table Configuration
        // Map TaskItem class to "Tasks" table in database
        // We use "TaskItem" in C# to avoid conflict with System.Threading.Tasks.Task
        // But in database, "Tasks" is a cleaner, more conventional name
        builder.ToTable("Tasks");

        // Primary Key
        builder.HasKey(t => t.Id);

        // Property Configurations

        // Title: Required short description of the task
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        // Description: Optional detailed description of requirements
        // Allows longer text for detailed task specifications
        builder.Property(t => t.Description)
            .HasMaxLength(2000);             // Not required - can be null

        // Status: Current workflow state of the task
        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>();           // Store TaskStatus enum as integer

        // Priority: Urgency level of the task
        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<int>();           // Store TaskPriority enum as integer

        // ProjectId: Required - every task must belong to a project
        builder.Property(t => t.ProjectId)
            .IsRequired();

        // AssigneeId: Optional - tasks can be unassigned
        // No additional configuration needed for nullable Guid

        // DueDate: Optional - not all tasks have deadlines
        // No additional configuration needed for nullable DateTime

        // Indexes for Query Performance

        // Index on ProjectId - frequently query "all tasks in a project"
        builder.HasIndex(t => t.ProjectId);

        // Index on AssigneeId - query "all tasks assigned to a user"
        builder.HasIndex(t => t.AssigneeId);

        // Index on Status - filter tasks by workflow state
        builder.HasIndex(t => t.Status);

        // Index on Priority - sort/filter by urgency
        builder.HasIndex(t => t.Priority);

        // Composite index for common queries: tasks by project and status
        // Optimizes Kanban board queries: "show me all InProgress tasks in this project"
        builder.HasIndex(t => new { t.ProjectId, t.Status });

        // Composite index for user's tasks by status
        // Optimizes personal dashboard: "show me my InProgress tasks"
        builder.HasIndex(t => new { t.AssigneeId, t.Status });

        // Relationship Configurations

        // Many-to-One: Tasks -> Project (Many tasks belong to one Project)
        builder.HasOne(t => t.Project)                      // Task belongs to one Project
            .WithMany(p => p.Tasks)                         // Project has many Tasks
            .HasForeignKey(t => t.ProjectId)                // Foreign key is ProjectId
            .OnDelete(DeleteBehavior.Cascade);              // If project deleted, delete all its tasks
                                                            // Cascade cleanup prevents orphaned tasks

        // Many-to-One: Tasks -> User (Many tasks assigned to one User)
        builder.HasOne(t => t.Assignee)                     // Task has one Assignee (can be null)
            .WithMany(u => u.AssignedTasks)                 // User has many AssignedTasks
            .HasForeignKey(t => t.AssigneeId)               // Foreign key is AssigneeId
            .OnDelete(DeleteBehavior.SetNull);              // If user deleted, unassign the task
                                                            // SetNull preserves task but removes assignment

        // One-to-Many: Task -> Comments (Task can have many Comments)
        builder.HasMany(t => t.Comments)                    // Task has many Comments
            .WithOne(c => c.Task)                           // Each Comment belongs to one Task
            .HasForeignKey(c => c.TaskId)                   // Foreign key is TaskId
            .OnDelete(DeleteBehavior.Cascade);              // If task deleted, delete all its comments
                                                            // Cascade ensures no orphaned comments
    }
}