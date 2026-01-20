using AutoMapper;
using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Comments.Commands.UpdateComment;

/// <summary>
/// Handler for UpdateCommentCommand.
/// Updates an existing comment.
/// </summary>
public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGenericRepository<Comment> _commentRepository;
    private readonly IGenericRepository<User> _userRepository;

    public UpdateCommentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IGenericRepository<Comment> commentRepository,
        IGenericRepository<User> userRepository)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
        _userRepository = userRepository;
    }

    public async Task<CommentDto> Handle(
        UpdateCommentCommand request,
        CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to update comments");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Get the comment
        var comment = await _commentRepository.GetByIdAsync(request.CommentId, cancellationToken);

        if (comment == null)
        {
            throw new ArgumentException($"Comment with ID {request.CommentId} not found");
        }

        // Check if user is the author
        if (comment.AuthorId != currentUserId)
        {
            throw new UnauthorizedAccessException("Only the comment author can update this comment");
        }

        // Update comment
        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get author details
        var author = await _userRepository.GetByIdAsync(comment.AuthorId, cancellationToken);

        // Return DTO
        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            TaskId = comment.TaskId,
            AuthorId = comment.AuthorId,
            AuthorName = author?.FullName ?? "Unknown",
            AuthorEmail = author?.Email ?? "",
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}
