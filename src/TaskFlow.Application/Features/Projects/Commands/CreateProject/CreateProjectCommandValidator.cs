using FluentValidation;

namespace TaskFlow.Application.Features.Projects.Commands.CreateProject;

/// <summary>
/// Validator for CreateProjectCommand.
/// Validates project creation data.
/// </summary>
public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        // Name is required
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Project name is required")
            .MaximumLength(200)
            .WithMessage("Project name must not exceed 200 characters");

        // Description is optional but has max length
        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Project description must not exceed 2000 characters");
        });

        // If StartDate is provided, validate it
        When(x => x.StartDate.HasValue, () =>
        {
            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1))
                .WithMessage("Start date cannot be more than 1 year in the future");
        });

        // If DueDate is provided, validate it
        When(x => x.DueDate.HasValue, () =>
        {
            RuleFor(x => x.DueDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Due date must be in the future");
        });

        // If both dates provided, DueDate must be after StartDate
        When(x => x.StartDate.HasValue && x.DueDate.HasValue, () =>
        {
            RuleFor(x => x.DueDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("Due date must be after start date");
        });

        // Status must be valid enum value
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid project status");
    }
}