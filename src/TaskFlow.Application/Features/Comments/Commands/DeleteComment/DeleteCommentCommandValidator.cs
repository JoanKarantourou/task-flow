using FluentValidation;

namespace TaskFlow.Application.Features.Comments.Commands.DeleteComment;

/// <summary>
/// Validator for DeleteCommentCommand.
/// </summary>
public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty()
            .WithMessage("Comment ID is required");
    }
}
