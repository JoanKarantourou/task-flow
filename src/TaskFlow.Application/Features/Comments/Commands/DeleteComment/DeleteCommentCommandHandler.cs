using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Comments.Commands.DeleteComment;

/// <summary>
/// Handler for DeleteCommentCommand.
/// Deletes a comment.
/// </summary>
public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGenericRepository<Comment> _commentRepository;

    public DeleteCommentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IGenericRepository<Comment> commentRepository)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
    }

    public async Task<bool> Handle(
        DeleteCommentCommand request,
        CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to delete comments");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Get the comment
        var comment = await _commentRepository.GetByIdAsync(request.CommentId, cancellationToken);

        if (comment == null)
        {
            throw new ArgumentException($"Comment with ID {request.CommentId} not found");
        }

        // Get the task and project to check permissions
        var task = await _unitOfWork.Tasks.GetByIdAsync(comment.TaskId, cancellationToken);

        if (task == null)
        {
            throw new ArgumentException($"Task not found for comment {request.CommentId}");
        }

        var project = await _unitOfWork.Projects.GetByIdAsync(task.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new ArgumentException($"Project not found for task {task.Id}");
        }

        // Check if user is the author or project owner
        bool isAuthor = comment.AuthorId == currentUserId;
        bool isProjectOwner = project.OwnerId == currentUserId;

        if (!isAuthor && !isProjectOwner)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this comment");
        }

        // Delete the comment
        _commentRepository.Delete(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
