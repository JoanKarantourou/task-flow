using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Persistence;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for TaskItem entity with task-specific queries.
/// Inherits common CRUD operations from GenericRepository.
/// Adds specialized methods for task-related business logic.
/// </summary>
public class TaskRepository : GenericRepository<TaskItem>, ITaskRepository
{
    /// <summary>
    /// Constructor - receives DbContext from DI and passes to base class.
    /// </summary>
    public TaskRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Retrieves a task by its Id with all related entities loaded (eager loading).
    /// Includes: Project, Assignee, and Comments (with comment authors).
    /// Prevents N+1 query problems.
    /// </summary>
    public async Task<TaskItem?> GetTaskWithDetailsAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Project)           // Load project
            .Include(t => t.Assignee)          // Load assignee
            .Include(t => t.Comments)          // Load comments
                .ThenInclude(c => c.Author)    // Load comment authors
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
    }

    /// <summary>
    /// Retrieves all tasks for a specific project.
    /// Includes assignee information to show who's working on each task.
    /// </summary>
    public async Task<IReadOnlyList<TaskItem>> GetTasksByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.Assignee)
            .OrderBy(t => t.Status)           // Group by status
                .ThenByDescending(t => t.Priority) // Then by priority
                .ThenBy(t => t.DueDate)       // Then by due date
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all tasks assigned to a specific user.
    /// Includes project information to show task context.
    /// </summary>
    public async Task<IReadOnlyList<TaskItem>> GetTasksByAssigneeAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.AssigneeId == userId)
            .Include(t => t.Project)
            .OrderBy(t => t.Status)
                .ThenByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves tasks filtered by status.
    /// Optionally filters by project or assignee.
    /// </summary>
    public async Task<IReadOnlyList<TaskItem>> GetTasksByStatusAsync(
        TaskStatus status,
        Guid? projectId = null,
        Guid? assigneeId = null,
        CancellationToken cancellationToken = default)
    {
        // Start with base query - apply ALL Where clauses BEFORE Include
        var query = _dbSet.Where(t => t.Status == status);

        // Apply optional filters BEFORE Include
        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        if (assigneeId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == assigneeId.Value);
        }

        // Now add Include statements AFTER all Where clauses
        return await query
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves tasks filtered by priority level.
    /// Optionally filters by project or assignee.
    /// </summary>
    public async Task<IReadOnlyList<TaskItem>> GetTasksByPriorityAsync(
        TaskPriority priority,
        Guid? projectId = null,
        Guid? assigneeId = null,
        CancellationToken cancellationToken = default)
    {
        // Start with base query - apply ALL Where clauses BEFORE Include
        var query = _dbSet.Where(t => t.Priority == priority);

        // Apply optional filters BEFORE Include
        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        if (assigneeId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == assigneeId.Value);
        }

        // Now add Include statements AFTER all Where clauses
        return await query
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .OrderBy(t => t.DueDate)
            .ThenBy(t => t.Status)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves tasks that are overdue (past their due date and not completed).
    /// Optionally filters by project or assignee.
    /// </summary>
    public async Task<IReadOnlyList<TaskItem>> GetOverdueTasksAsync(
        Guid? projectId = null,
        Guid? assigneeId = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // Start with base query - apply ALL Where clauses BEFORE Include
        var query = _dbSet.Where(t => t.DueDate.HasValue &&
                                     t.DueDate.Value < now &&
                                     t.Status != TaskStatus.Done &&
                                     t.Status != TaskStatus.Cancelled);

        // Apply optional filters BEFORE Include
        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        if (assigneeId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == assigneeId.Value);
        }

        // Now add Include statements AFTER all Where clauses
        return await query
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .OrderBy(t => t.DueDate) // Most overdue first
            .ThenByDescending(t => t.Priority)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves tasks due within a specified date range.
    /// Useful for sprint planning and weekly task views.
    /// </summary>
    public async Task<IReadOnlyList<TaskItem>> GetTasksDueInRangeAsync(
        DateTime startDate,
        DateTime endDate,
        Guid? projectId = null,
        Guid? assigneeId = null,
        CancellationToken cancellationToken = default)
    {
        // Start with base query - apply ALL Where clauses BEFORE Include
        var query = _dbSet.Where(t => t.DueDate.HasValue &&
                                     t.DueDate.Value >= startDate &&
                                     t.DueDate.Value <= endDate);

        // Apply optional filters BEFORE Include
        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        if (assigneeId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == assigneeId.Value);
        }

        // Now add Include statements AFTER all Where clauses
        return await query
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Searches tasks by title or description.
    /// Performs a case-insensitive search.
    /// Optionally filters by project or assignee.
    /// </summary>
    public async Task<IReadOnlyList<TaskItem>> SearchTasksAsync(
        string searchTerm,
        Guid? projectId = null,
        Guid? assigneeId = null,
        CancellationToken cancellationToken = default)
    {
        // Start with base query
        var query = _dbSet.AsQueryable();

        // Apply search filter BEFORE Include
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(lowerSearchTerm) ||
                                   (t.Description != null && t.Description.ToLower().Contains(lowerSearchTerm)));
        }

        // Apply optional filters BEFORE Include
        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        if (assigneeId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == assigneeId.Value);
        }

        // Now add Include statements AFTER all Where clauses
        return await query
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the count of tasks grouped by status for a project.
    /// Used for Kanban board column counts and progress visualization.
    /// </summary>
    public async Task<Dictionary<string, int>> GetTaskCountsByStatusAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var tasks = await _dbSet
            .Where(t => t.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        var counts = new Dictionary<string, int>
        {
            ["Todo"] = tasks.Count(t => t.Status == TaskStatus.Todo),
            ["InProgress"] = tasks.Count(t => t.Status == TaskStatus.InProgress),
            ["InReview"] = tasks.Count(t => t.Status == TaskStatus.InReview),
            ["Done"] = tasks.Count(t => t.Status == TaskStatus.Done),
            ["Cancelled"] = tasks.Count(t => t.Status == TaskStatus.Cancelled)
        };

        return counts;
    }

    /// <summary>
    /// Checks if a user has permission to modify a specific task.
    /// Returns true if the user is the task assignee, project owner, or project admin.
    /// </summary>
    public async Task<bool> UserCanModifyTaskAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var task = await _dbSet
            .Include(t => t.Project)
                .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
        {
            return false;
        }

        // User can modify if they are:
        // 1. The task assignee
        // 2. The project owner
        // 3. A project admin
        return task.AssigneeId == userId ||
               task.Project.OwnerId == userId ||
               task.Project.Members.Any(m => m.UserId == userId &&
                                            (m.Role == "Admin" || m.Role == "Owner"));
    }

    /// <summary>
    /// Retrieves tasks with filtering, sorting, and pagination.
    /// Only returns tasks from projects the user has access to.
    /// </summary>
    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetTasksPagedAsync(
        TaskFilterParameters parameters,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Start with base query - only tasks from projects user has access to
        var query = _dbSet
            .Include(t => t.Project)
                .ThenInclude(p => p.Members)
            .Where(t => t.Project.OwnerId == userId ||
                       t.Project.Members.Any(m => m.UserId == userId));

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm.ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(searchTerm) ||
                                    (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
        }

        // Apply status filter
        if (parameters.Status.HasValue)
        {
            query = query.Where(t => t.Status == parameters.Status.Value);
        }

        // Apply priority filter
        if (parameters.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == parameters.Priority.Value);
        }

        // Apply project filter
        if (parameters.ProjectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == parameters.ProjectId.Value);
        }

        // Apply assignee filter
        if (parameters.AssigneeId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == parameters.AssigneeId.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = parameters.SortBy?.ToLower() switch
        {
            "title" => parameters.SortDescending
                ? query.OrderByDescending(t => t.Title)
                : query.OrderBy(t => t.Title),
            "priority" => parameters.SortDescending
                ? query.OrderByDescending(t => t.Priority)
                : query.OrderBy(t => t.Priority),
            "status" => parameters.SortDescending
                ? query.OrderByDescending(t => t.Status)
                : query.OrderBy(t => t.Status),
            "duedate" => parameters.SortDescending
                ? query.OrderByDescending(t => t.DueDate)
                : query.OrderBy(t => t.DueDate),
            _ => parameters.SortDescending
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.CreatedAt)
        };

        // Apply pagination
        var items = await query
            .Include(t => t.Assignee)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}