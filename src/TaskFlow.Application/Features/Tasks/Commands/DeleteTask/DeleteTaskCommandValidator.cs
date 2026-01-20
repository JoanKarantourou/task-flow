using FluentValidation;

namespace TaskFlow.Application.Features.Tasks.Commands.DeleteTask;

/// <summary>
/// Validator for DeleteTaskCommand.
/// </summary>
public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    public DeleteTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("Task ID is required");
    }
}
