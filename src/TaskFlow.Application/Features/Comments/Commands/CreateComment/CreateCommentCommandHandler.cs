using AutoMapper;
using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Comments.Commands.CreateComment;

/// <summary>
/// Handler for CreateCommentCommand.
/// Creates a new comment on a task.
/// </summary>
public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGenericRepository<Comment> _commentRepository;
    private readonly IGenericRepository<User> _userRepository;

    public CreateCommentCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IGenericRepository<Comment> commentRepository,
        IGenericRepository<User> userRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
        _userRepository = userRepository;
    }

    public async Task<CommentDto> Handle(
        CreateCommentCommand request,
        CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to create comments");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Verify the task exists and user has access
        var task = await _unitOfWork.Tasks.GetTaskWithDetailsAsync(request.TaskId, cancellationToken);

        if (task == null)
        {
            throw new ArgumentException($"Task with ID {request.TaskId} not found");
        }

        // Verify user has access to the project
        var project = await _unitOfWork.Projects.GetByIdAsync(task.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new ArgumentException($"Project not found for task {request.TaskId}");
        }

        bool hasAccess = project.OwnerId == currentUserId || task.AssigneeId == currentUserId;

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("You don't have permission to comment on this task");
        }

        // Get the current user for author details
        var author = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);

        if (author == null)
        {
            throw new ArgumentException("Current user not found");
        }

        // Create the comment
        var comment = new Comment
        {
            Content = request.Content,
            TaskId = request.TaskId,
            AuthorId = currentUserId
        };

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO with author details
        var commentDto = new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            TaskId = comment.TaskId,
            AuthorId = comment.AuthorId,
            AuthorName = author.FullName,
            AuthorEmail = author.Email,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };

        return commentDto;
    }
}
