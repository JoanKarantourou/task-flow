using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Repository interface for Project entity with project-specific queries.
/// Extends IGenericRepository to inherit common CRUD operations,
/// and adds specialized methods for project-related business logic.
/// </summary>
public interface IProjectRepository : IGenericRepository<Project>
{
    /// <summary>
    /// Retrieves a project by its Id with all related entities loaded.
    /// Includes: Owner, Tasks, and Members (with their User data).
    /// This is an "eager loading" query that prevents N+1 query problems.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The project with all related data, or null if not found</returns>
    /// <example>
    /// var project = await projectRepository.GetProjectWithDetailsAsync(projectId);
    /// // Can now access project.Owner, project.Tasks, project.Members without extra queries
    /// var ownerName = project.Owner.FullName; // No additional database call
    /// var taskCount = project.Tasks.Count; // No additional database call
    /// </example>
    Task<Project?> GetProjectWithDetailsAsync(
        Guid projectId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all projects owned by a specific user.
    /// Includes basic project information without related entities.
    /// </summary>
    /// <param name="userId">The unique identifier of the project owner</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of projects owned by the user</returns>
    /// <example>
    /// // Get all projects I own
    /// var myProjects = await projectRepository.GetProjectsByOwnerAsync(currentUserId);
    /// </example>
    Task<IReadOnlyList<Project>> GetProjectsByOwnerAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all projects where the user is a member (including as owner).
    /// This is different from GetProjectsByOwnerAsync - it includes projects
    /// where the user is a member but not the owner.
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of projects where the user is a member</returns>
    /// <example>
    /// // Get all projects I'm involved in (owner or member)
    /// var myProjects = await projectRepository.GetProjectsByMemberAsync(currentUserId);
    /// </example>
    Task<IReadOnlyList<Project>> GetProjectsByMemberAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves projects filtered by their status.
    /// Useful for dashboard views showing active, completed, or archived projects.
    /// </summary>
    /// <param name="status">The project status to filter by</param>
    /// <param name="userId">Optional: filter by user (shows only their projects)</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of projects matching the status filter</returns>
    /// <example>
    /// // Get all active projects for current user
    /// var activeProjects = await projectRepository.GetProjectsByStatusAsync(
    ///     ProjectStatus.Active, 
    ///     currentUserId);
    /// 
    /// // Get all completed projects (all users)
    /// var completedProjects = await projectRepository.GetProjectsByStatusAsync(
    ///     ProjectStatus.Completed);
    /// </example>
    Task<IReadOnlyList<Project>> GetProjectsByStatusAsync(
        ProjectStatus status, 
        Guid? userId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has access to a specific project.
    /// Returns true if the user is either the owner or a member of the project.
    /// Used for authorization checks before allowing project operations.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>True if user has access, false otherwise</returns>
    /// <example>
    /// // Check if user can access project before showing details
    /// var hasAccess = await projectRepository.UserHasAccessToProjectAsync(projectId, userId);
    /// if (!hasAccess)
    /// {
    ///     throw new UnauthorizedAccessException("You don't have access to this project");
    /// }
    /// </example>
    Task<bool> UserHasAccessToProjectAsync(
        Guid projectId, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets statistics for a project including task counts by status.
    /// Returns a summary object with total tasks, completed tasks, etc.
    /// Useful for dashboard displays and progress tracking.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Dictionary with statistic key-value pairs</returns>
    /// <example>
    /// var stats = await projectRepository.GetProjectStatisticsAsync(projectId);
    /// // stats might contain: { "TotalTasks": 50, "CompletedTasks": 30, "InProgressTasks": 15, ... }
    /// var completionPercentage = (stats["CompletedTasks"] / stats["TotalTasks"]) * 100;
    /// </example>
    Task<Dictionary<string, int>> GetProjectStatisticsAsync(
        Guid projectId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches projects by name or description.
    /// Performs a case-insensitive search across project names and descriptions.
    /// Optionally filters by user to search only their projects.
    /// </summary>
    /// <param name="searchTerm">The text to search for</param>
    /// <param name="userId">Optional: limit search to projects accessible by this user</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of projects matching the search criteria</returns>
    /// <example>
    /// // Search my projects for "mobile app"
    /// var results = await projectRepository.SearchProjectsAsync("mobile app", currentUserId);
    /// </example>
    Task<IReadOnlyList<Project>> SearchProjectsAsync(
        string searchTerm, 
        Guid? userId = null, 
        CancellationToken cancellationToken = default);
}
