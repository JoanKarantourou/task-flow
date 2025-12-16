using FluentValidation;

namespace TaskFlow.Application.Features.Tasks.Commands.UpdateTask;

/// <summary>
/// Validator for UpdateTaskCommand.
/// Validates update data before processing.
/// </summary>
public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        // TaskId must be provided
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("Task ID is required");

        // If Title is provided, validate it
        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title)
                .MaximumLength(200)
                .WithMessage("Title must not exceed 200 characters");
        });

        // If Description is provided, validate it
        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Description must not exceed 2000 characters");
        });

        // If DueDate is provided, validate it's in the future
        When(x => x.DueDate.HasValue, () =>
        {
            RuleFor(x => x.DueDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Due date must be in the future");
        });
    }
}