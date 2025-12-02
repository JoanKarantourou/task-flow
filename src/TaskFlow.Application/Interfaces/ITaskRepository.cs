using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Repository interface for TaskItem entity with task-specific queries.
/// Extends IGenericRepository to inherit common CRUD operations,
/// and adds specialized methods for task-related business logic.
/// </summary>
public interface ITaskRepository : IGenericRepository<TaskItem>
{
    /// <summary>
    /// Retrieves a task by its Id with all related entities loaded.
    /// Includes: Project, Assignee, and Comments (with comment authors).
    /// This prevents N+1 query problems when displaying task details.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The task with all related data, or null if not found</returns>
    /// <example>
    /// var task = await taskRepository.GetTaskWithDetailsAsync(taskId);
    /// // Can now access related data without additional queries
    /// var projectName = task.Project.Name;
    /// var assigneeName = task.Assignee?.FullName;
    /// var commentCount = task.Comments.Count;
    /// </example>
    Task<TaskItem?> GetTaskWithDetailsAsync(
        Guid taskId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all tasks for a specific project.
    /// Includes assignee information to show who's working on each task.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of tasks in the project</returns>
    /// <example>
    /// // Get all tasks for a project to display in Kanban board
    /// var tasks = await taskRepository.GetTasksByProjectAsync(projectId);
    /// var todoTasks = tasks.Where(t => t.Status == TaskStatus.Todo);
    /// </example>
    Task<IReadOnlyList<TaskItem>> GetTasksByProjectAsync(
        Guid projectId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all tasks assigned to a specific user.
    /// Includes project information to show task context.
    /// Useful for personal task lists and dashboards.
    /// </summary>
    /// <param name="userId">The unique identifier of the assignee</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of tasks assigned to the user</returns>
    /// <example>
    /// // Get my assigned tasks
    /// var myTasks = await taskRepository.GetTasksByAssigneeAsync(currentUserId);
    /// </example>
    Task<IReadOnlyList<TaskItem>> GetTasksByAssigneeAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves tasks filtered by status.
    /// Optionally filters by project or assignee.
    /// Used for Kanban boards, status reports, and filtered task lists.
    /// </summary>
    /// <param name="status">The task status to filter by</param>
    /// <param name="projectId">Optional: filter by specific project</param>
    /// <param name="assigneeId">Optional: filter by specific assignee</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of tasks matching the filters</returns>
    /// <example>
    /// // Get all "In Progress" tasks for a project
    /// var inProgressTasks = await taskRepository.GetTasksByStatusAsync(
    ///     TaskStatus.InProgress, 
    ///     projectId: projectId);
    /// 
    /// // Get all my completed tasks
    /// var myCompletedTasks = await taskRepository.GetTasksByStatusAsync(
    ///     TaskStatus.Done, 
    ///     assigneeId: currentUserId);
    /// </example>
    Task<IReadOnlyList<TaskItem>> GetTasksByStatusAsync(
        TaskStatus status, 
        Guid? projectId = null, 
        Guid? assigneeId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves tasks filtered by priority level.
    /// Optionally filters by project or assignee.
    /// Used for priority-based task lists and urgent task reports.
    /// </summary>
    /// <param name="priority">The task priority to filter by</param>
    /// <param name="projectId">Optional: filter by specific project</param>
    /// <param name="assigneeId">Optional: filter by specific assignee</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of tasks matching the filters</returns>
    /// <example>
    /// // Get all critical priority tasks in a project
    /// var criticalTasks = await taskRepository.GetTasksByPriorityAsync(
    ///     TaskPriority.Critical, 
    ///     projectId: projectId);
    /// 
    /// // Get all my high priority tasks
    /// var myHighPriorityTasks = await taskRepository.GetTasksByPriorityAsync(
    ///     TaskPriority.High, 
    ///     assigneeId: currentUserId);
    /// </example>
    Task<IReadOnlyList<TaskItem>> GetTasksByPriorityAsync(
        TaskPriority priority, 
        Guid? projectId = null, 
        Guid? assigneeId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves tasks that are overdue (past their due date and not completed).
    /// Optionally filters by project or assignee.
    /// Used for overdue task reports and alerts.
    /// </summary>
    /// <param name="projectId">Optional: filter by specific project</param>
    /// <param name="assigneeId">Optional: filter by specific assignee</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of overdue tasks</returns>
    /// <example>
    /// // Get all overdue tasks in a project
    /// var overdueTasks = await taskRepository.GetOverdueTasksAsync(projectId: projectId);
    /// 
    /// // Get my overdue tasks
    /// var myOverdueTasks = await taskRepository.GetOverdueTasksAsync(assigneeId: currentUserId);
    /// </example>
    Task<IReadOnlyList<TaskItem>> GetOverdueTasksAsync(
        Guid? projectId = null, 
        Guid? assigneeId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves tasks due within a specified date range.
    /// Useful for sprint planning, weekly task views, and deadline tracking.
    /// </summary>
    /// <param name="startDate">Start of the date range</param>
    /// <param name="endDate">End of the date range</param>
    /// <param name="projectId">Optional: filter by specific project</param>
    /// <param name="assigneeId">Optional: filter by specific assignee</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of tasks due within the date range</returns>
    /// <example>
    /// // Get tasks due this week
    /// var thisWeekTasks = await taskRepository.GetTasksDueInRangeAsync(
    ///     DateTime.UtcNow, 
    ///     DateTime.UtcNow.AddDays(7));
    /// 
    /// // Get my tasks due in the next 3 days
    /// var upcomingTasks = await taskRepository.GetTasksDueInRangeAsync(
    ///     DateTime.UtcNow, 
    ///     DateTime.UtcNow.AddDays(3),
    ///     assigneeId: currentUserId);
    /// </example>
    Task<IReadOnlyList<TaskItem>> GetTasksDueInRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        Guid? projectId = null, 
        Guid? assigneeId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches tasks by title or description.
    /// Performs a case-insensitive search across task titles and descriptions.
    /// Optionally filters by project or assignee.
    /// </summary>
    /// <param name="searchTerm">The text to search for</param>
    /// <param name="projectId">Optional: limit search to specific project</param>
    /// <param name="assigneeId">Optional: limit search to specific assignee's tasks</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of tasks matching the search criteria</returns>
    /// <example>
    /// // Search for tasks containing "bug fix" in my project
    /// var bugTasks = await taskRepository.SearchTasksAsync("bug fix", projectId: projectId);
    /// 
    /// // Search my tasks for "authentication"
    /// var authTasks = await taskRepository.SearchTasksAsync(
    ///     "authentication", 
    ///     assigneeId: currentUserId);
    /// </example>
    Task<IReadOnlyList<TaskItem>> SearchTasksAsync(
        string searchTerm, 
        Guid? projectId = null, 
        Guid? assigneeId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of tasks grouped by status for a project.
    /// Used for Kanban board column counts and project progress visualization.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Dictionary with status as key and count as value</returns>
    /// <example>
    /// var statusCounts = await taskRepository.GetTaskCountsByStatusAsync(projectId);
    /// // Result: { "Todo": 5, "InProgress": 3, "InReview": 2, "Done": 10 }
    /// var totalTasks = statusCounts.Values.Sum();
    /// var completionRate = (statusCounts["Done"] / totalTasks) * 100;
    /// </example>
    Task<Dictionary<string, int>> GetTaskCountsByStatusAsync(
        Guid projectId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has permission to modify a specific task.
    /// Returns true if the user is the task assignee, project owner, or project admin.
    /// Used for authorization checks before allowing task modifications.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task</param>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>True if user can modify the task, false otherwise</returns>
    /// <example>
    /// // Check if user can edit task before processing update
    /// var canEdit = await taskRepository.UserCanModifyTaskAsync(taskId, userId);
    /// if (!canEdit)
    /// {
    ///     throw new UnauthorizedAccessException("You don't have permission to modify this task");
    /// }
    /// </example>
    Task<bool> UserCanModifyTaskAsync(
        Guid taskId, 
        Guid userId, 
        CancellationToken cancellationToken = default);
}
