using FluentValidation;

namespace TaskFlow.Application.Features.Comments.Commands.CreateComment;

/// <summary>
/// Validator for CreateCommentCommand.
/// </summary>
public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("Task ID is required");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Comment content is required")
            .MaximumLength(5000)
            .WithMessage("Comment must not exceed 5000 characters");
    }
}
