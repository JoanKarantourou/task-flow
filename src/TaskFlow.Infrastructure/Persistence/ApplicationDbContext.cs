using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence;

/// <summary>
/// Main database context for the TaskFlow application.
/// Represents a session with the database and provides DbSet properties for each entity.
/// Inherits from DbContext which is EF Core's base class for database operations.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Constructor that accepts DbContextOptions.
    /// Options include connection string, provider (PostgreSQL), and other configurations.
    /// These options are typically configured in Program.cs during DI setup.
    /// </summary>
    /// <param name="options">Configuration options for this context</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSet Properties - Each represents a table in the database
    // DbSet<T> provides LINQ query capabilities (Where, Select, Include, etc.)

    /// <summary>
    /// Users table - stores all user accounts in the system.
    /// Use this to query, add, update, or delete users.
    /// Example: await Users.Where(u => u.Email == email).FirstOrDefaultAsync();
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Projects table - stores all projects created by users.
    /// Each project belongs to an owner and can have multiple members.
    /// Example: await Projects.Include(p => p.Tasks).ToListAsync();
    /// </summary>
    public DbSet<Project> Projects { get; set; } = null!;

    /// <summary>
    /// Tasks table - stores all tasks within projects.
    /// Named "Tasks" in C# but will map to "TaskItems" table to avoid SQL keyword conflict.
    /// Example: await Tasks.Where(t => t.Status == TaskStatus.InProgress).ToListAsync();
    /// </summary>
    public DbSet<TaskItem> Tasks { get; set; } = null!;

    /// <summary>
    /// Comments table - stores all comments on tasks.
    /// Used for team collaboration and task discussions.
    /// Example: await Comments.Where(c => c.TaskId == taskId).ToListAsync();
    /// </summary>
    public DbSet<Comment> Comments { get; set; } = null!;

    /// <summary>
    /// ProjectMembers table - join table for many-to-many relationship between users and projects.
    /// Tracks which users belong to which projects and their roles.
    /// Example: await ProjectMembers.Where(pm => pm.UserId == userId).ToListAsync();
    /// </summary>
    public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;

    /// <summary>
    /// Configures the model using Fluent API.
    /// This method is called by EF Core when the model is being created.
    /// We use this to:
    /// - Apply entity configurations (from separate configuration classes)
    /// - Define relationships between entities
    /// - Set up indexes, constraints, and table names
    /// </summary>
    /// <param name="modelBuilder">Builder used to construct the model for this context</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Call base implementation first (important for proper EF Core functioning)
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the current assembly
        // This will find and apply all classes implementing IEntityTypeConfiguration<T>
        // We'll create these configuration classes in the next step (5.4)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Additional global configurations can go here if needed
        // For example: setting default schema, conventions, etc.
    }

    /// <summary>
    /// Overrides SaveChanges to automatically update timestamp properties.
    /// This ensures CreatedAt and UpdatedAt are always set correctly without manual intervention.
    /// Called whenever you call SaveChanges() or SaveChangesAsync().
    /// </summary>
    /// <returns>Number of state entries written to the database</returns>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Async version of SaveChanges with automatic timestamp updates.
    /// This is the preferred method for saving changes in async/await contexts.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Number of state entries written to the database</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Private helper method to update CreatedAt and UpdatedAt timestamps.
    /// Automatically sets timestamps based on entity state:
    /// - Added: Sets both CreatedAt and UpdatedAt to current UTC time
    /// - Modified: Updates only UpdatedAt to current UTC time
    /// This eliminates the need to manually set these fields throughout the application.
    /// </summary>
    private void UpdateTimestamps()
    {
        // Get all entities that inherit from BaseEntity and are in Added or Modified state
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.BaseEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (Domain.Common.BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                // New entity - set both timestamps
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                // Existing entity being updated - only update UpdatedAt
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}