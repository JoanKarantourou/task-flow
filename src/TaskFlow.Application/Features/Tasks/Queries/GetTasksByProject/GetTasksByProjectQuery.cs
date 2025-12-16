using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Tasks.Queries.GetTasksByProject;

/// <summary>
/// Query to get all tasks for a specific project.
/// User must have access to the project.
/// </summary>
public class GetTasksByProjectQuery : IRequest<List<TaskDto>>
{
    /// <summary>
    /// ID of the project to get tasks for.
    /// </summary>
    public Guid ProjectId { get; set; }
}