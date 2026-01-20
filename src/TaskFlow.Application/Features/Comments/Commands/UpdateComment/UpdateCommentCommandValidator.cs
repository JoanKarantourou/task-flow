using FluentValidation;

namespace TaskFlow.Application.Features.Comments.Commands.UpdateComment;

/// <summary>
/// Validator for UpdateCommentCommand.
/// </summary>
public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty()
            .WithMessage("Comment ID is required");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Comment content is required")
            .MaximumLength(5000)
            .WithMessage("Comment must not exceed 5000 characters");
    }
}
