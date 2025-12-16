using AutoMapper;
using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Features.Tasks.Commands.UpdateTask;

/// <summary>
/// Handler for UpdateTaskCommand.
/// Updates a task with authorization checks.
/// </summary>
public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTaskCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Get the task with details
        var task = await _unitOfWork.Tasks.GetTaskWithDetailsAsync(request.TaskId, cancellationToken);

        if (task == null)
        {
            throw new ArgumentException($"Task with ID {request.TaskId} not found");
        }

        // Check if user can modify this task
        var canModify = await _unitOfWork.Tasks.UserCanModifyTaskAsync(
            request.TaskId,
            currentUserId,
            cancellationToken);

        if (!canModify)
        {
            throw new UnauthorizedAccessException("You don't have permission to modify this task");
        }

        // Update task properties (only if provided)
        if (request.Title != null)
            task.Title = request.Title;

        if (request.Description != null)
            task.Description = request.Description;

        if (request.Status.HasValue)
            task.Status = request.Status.Value;

        if (request.Priority.HasValue)
            task.Priority = request.Priority.Value;

        if (request.AssigneeId.HasValue)
        {
            // Validate assignee has access to project
            var assigneeHasAccess = await _unitOfWork.Projects.UserHasAccessToProjectAsync(
                task.ProjectId,
                request.AssigneeId.Value,
                cancellationToken);

            if (!assigneeHasAccess)
            {
                throw new ArgumentException("Assignee must be a member of the project");
            }

            task.AssigneeId = request.AssigneeId;
        }

        if (request.DueDate.HasValue)
            task.DueDate = request.DueDate;

        // Save changes
        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch updated task with details
        var updatedTask = await _unitOfWork.Tasks.GetTaskWithDetailsAsync(task.Id, cancellationToken);

        // TODO: Phase 6 - Publish TaskUpdatedEvent

        return _mapper.Map<TaskDto>(updatedTask);
    }
}