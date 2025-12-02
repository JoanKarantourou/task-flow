using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Persistence;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Project entity with project-specific queries.
/// Inherits common CRUD operations from GenericRepository.
/// Adds specialized methods for project-related business logic.
/// </summary>
public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    /// <summary>
    /// Constructor - receives DbContext from DI and passes to base class.
    /// </summary>
    public ProjectRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Retrieves a project by its Id with all related entities loaded (eager loading).
    /// Includes: Owner, Tasks, and Members (with their User data).
    /// Prevents N+1 query problems by loading everything in one query.
    /// </summary>
    public async Task<Project?> GetProjectWithDetailsAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Owner)              // Load project owner
            .Include(p => p.Tasks)              // Load all tasks
                .ThenInclude(t => t.Assignee)   // Load task assignees
            .Include(p => p.Members)            // Load project members
                .ThenInclude(pm => pm.User)     // Load member user details
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
    }

    /// <summary>
    /// Retrieves all projects owned by a specific user.
    /// </summary>
    public async Task<IReadOnlyList<Project>> GetProjectsByOwnerAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.OwnerId == userId)
            .Include(p => p.Owner)
            .Include(p => p.Tasks)
            .OrderByDescending(p => p.CreatedAt) // Most recent first
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all projects where the user is a member (including as owner).
    /// Checks both ownership and membership.
    /// </summary>
    public async Task<IReadOnlyList<Project>> GetProjectsByMemberAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId))
            .Include(p => p.Owner)
            .Include(p => p.Tasks)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves projects filtered by their status.
    /// Optionally filters by user (shows only their projects).
    /// </summary>
    public async Task<IReadOnlyList<Project>> GetProjectsByStatusAsync(
        ProjectStatus status,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        // Start with base query - apply ALL Where clauses BEFORE Include
        var query = _dbSet.Where(p => p.Status == status);

        // If userId provided, add user filter BEFORE Include
        if (userId.HasValue)
        {
            query = query.Where(p => p.OwnerId == userId.Value ||
                                   p.Members.Any(m => m.UserId == userId.Value));
        }

        // Now add Include statements AFTER all Where clauses
        return await query
            .Include(p => p.Owner)
            .Include(p => p.Tasks)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a user has access to a specific project.
    /// Returns true if the user is either the owner or a member of the project.
    /// </summary>
    public async Task<bool> UserHasAccessToProjectAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(p => p.Id == projectId &&
                          (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)),
                     cancellationToken);
    }

    /// <summary>
    /// Gets statistics for a project including task counts by status.
    /// Returns a dictionary with statistic key-value pairs.
    /// </summary>
    public async Task<Dictionary<string, int>> GetProjectStatisticsAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await _dbSet
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            return new Dictionary<string, int>();
        }

        var stats = new Dictionary<string, int>
        {
            ["TotalTasks"] = project.Tasks.Count,
            ["TodoTasks"] = project.Tasks.Count(t => t.Status == TaskStatus.Todo),
            ["InProgressTasks"] = project.Tasks.Count(t => t.Status == TaskStatus.InProgress),
            ["InReviewTasks"] = project.Tasks.Count(t => t.Status == TaskStatus.InReview),
            ["CompletedTasks"] = project.Tasks.Count(t => t.Status == TaskStatus.Done),
            ["CancelledTasks"] = project.Tasks.Count(t => t.Status == TaskStatus.Cancelled),
            ["HighPriorityTasks"] = project.Tasks.Count(t => t.Priority == TaskPriority.High ||
                                                             t.Priority == TaskPriority.Critical),
            ["OverdueTasks"] = project.Tasks.Count(t => t.DueDate.HasValue &&
                                                        t.DueDate.Value < DateTime.UtcNow &&
                                                        t.Status != TaskStatus.Done &&
                                                        t.Status != TaskStatus.Cancelled)
        };

        return stats;
    }

    /// <summary>
    /// Searches projects by name or description.
    /// Performs a case-insensitive search.
    /// Optionally filters by user to search only their projects.
    /// </summary>
    public async Task<IReadOnlyList<Project>> SearchProjectsAsync(
        string searchTerm,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        // Start with base query
        var query = _dbSet.AsQueryable();

        // Apply search filter BEFORE Include
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(lowerSearchTerm) ||
                                   (p.Description != null && p.Description.ToLower().Contains(lowerSearchTerm)));
        }

        // Apply user filter BEFORE Include
        if (userId.HasValue)
        {
            query = query.Where(p => p.OwnerId == userId.Value ||
                                   p.Members.Any(m => m.UserId == userId.Value));
        }

        // Now add Include statements AFTER all Where clauses
        return await query
            .Include(p => p.Owner)
            .Include(p => p.Tasks)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(cancellationToken);
    }
}