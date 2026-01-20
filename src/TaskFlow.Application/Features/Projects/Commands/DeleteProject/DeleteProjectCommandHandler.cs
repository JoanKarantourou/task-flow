using MediatR;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Features.Projects.Commands.DeleteProject;

/// <summary>
/// Handler for DeleteProjectCommand.
/// Deletes a project and all associated data.
/// </summary>
public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteProjectCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(
        DeleteProjectCommand request,
        CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to delete projects");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Get the project
        var project = await _unitOfWork.Projects.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new ArgumentException($"Project with ID {request.ProjectId} not found");
        }

        // Check if user is the owner
        if (project.OwnerId != currentUserId)
        {
            throw new UnauthorizedAccessException("Only the project owner can delete this project");
        }

        // Delete the project (cascade delete will handle tasks and comments)
        _unitOfWork.Projects.Delete(project);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
