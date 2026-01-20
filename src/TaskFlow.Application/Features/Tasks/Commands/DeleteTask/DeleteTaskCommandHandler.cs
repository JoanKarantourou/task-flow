using MediatR;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Features.Tasks.Commands.DeleteTask;

/// <summary>
/// Handler for DeleteTaskCommand.
/// Deletes a task and all associated comments.
/// </summary>
public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteTaskCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(
        DeleteTaskCommand request,
        CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to delete tasks");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Get the task with project details
        var task = await _unitOfWork.Tasks.GetTaskWithDetailsAsync(request.TaskId, cancellationToken);

        if (task == null)
        {
            throw new ArgumentException($"Task with ID {request.TaskId} not found");
        }

        // Check if user has permission (project owner or task assignee)
        var project = await _unitOfWork.Projects.GetByIdAsync(task.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new ArgumentException($"Project not found for task {request.TaskId}");
        }

        bool isProjectOwner = project.OwnerId == currentUserId;
        bool isAssignee = task.AssigneeId == currentUserId;

        if (!isProjectOwner && !isAssignee)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this task");
        }

        // Delete the task (cascade delete will handle comments)
        _unitOfWork.Tasks.Delete(task);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
