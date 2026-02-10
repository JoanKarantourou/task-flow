using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.DTOs;

/// <summary>
/// Parameters for filtering and paginating tasks.
/// Used in GET /api/tasks endpoint.
/// </summary>
public class TaskFilterParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    /// <summary>
    /// Search term to filter tasks by title or description.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by task status.
    /// </summary>
    public TaskStatus? Status { get; set; }

    /// <summary>
    /// Filter by task priority.
    /// </summary>
    public TaskPriority? Priority { get; set; }

    /// <summary>
    /// Filter by project ID.
    /// </summary>
    public Guid? ProjectId { get; set; }

    /// <summary>
    /// Filter by assignee ID.
    /// </summary>
    public Guid? AssigneeId { get; set; }

    /// <summary>
    /// Page number (1-based). Defaults to 1.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page. Defaults to 10, max 50.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    /// <summary>
    /// Field to sort by. Options: title, priority, status, dueDate, createdAt.
    /// Defaults to createdAt.
    /// </summary>
    public string SortBy { get; set; } = "createdAt";

    /// <summary>
    /// Sort in descending order. Defaults to true.
    /// </summary>
    public bool SortDescending { get; set; } = true;
}
