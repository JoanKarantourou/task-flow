namespace TaskFlow.Application.DTOs;

/// <summary>
/// Dashboard statistics DTO.
/// Contains aggregated stats for the dashboard overview.
/// </summary>
public class DashboardStatsDto
{
    /// <summary>
    /// Total number of projects the user has access to.
    /// </summary>
    public int TotalProjects { get; set; }

    /// <summary>
    /// Number of active projects.
    /// </summary>
    public int ActiveProjects { get; set; }

    /// <summary>
    /// Total number of tasks across all accessible projects.
    /// </summary>
    public int TotalTasks { get; set; }

    /// <summary>
    /// Number of pending tasks (Todo + InProgress).
    /// </summary>
    public int PendingTasks { get; set; }

    /// <summary>
    /// Number of completed tasks (Done).
    /// </summary>
    public int CompletedTasks { get; set; }

    /// <summary>
    /// Number of overdue tasks (past due date and not completed).
    /// </summary>
    public int OverdueTasks { get; set; }

    /// <summary>
    /// Task count grouped by status.
    /// Keys: "Todo", "InProgress", "InReview", "Done", "Cancelled"
    /// </summary>
    public Dictionary<string, int> TasksByStatus { get; set; } = new();

    /// <summary>
    /// Task count grouped by priority.
    /// Keys: "Low", "Medium", "High", "Critical"
    /// </summary>
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
}
