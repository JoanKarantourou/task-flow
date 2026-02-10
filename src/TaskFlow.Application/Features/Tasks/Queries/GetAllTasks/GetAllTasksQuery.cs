using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Tasks.Queries.GetAllTasks;

/// <summary>
/// Query to get all tasks with filtering and pagination.
/// Returns tasks from projects the user has access to.
/// </summary>
public class GetAllTasksQuery : IRequest<PagedResult<TaskDto>>
{
    /// <summary>
    /// Filter and pagination parameters.
    /// </summary>
    public TaskFilterParameters Parameters { get; set; } = new();
}
