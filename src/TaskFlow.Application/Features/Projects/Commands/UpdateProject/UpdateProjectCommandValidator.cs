using FluentValidation;

namespace TaskFlow.Application.Features.Projects.Commands.UpdateProject;

/// <summary>
/// Validator for UpdateProjectCommand.
/// Validates project update data.
/// </summary>
public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        // ProjectId is required
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required");

        // Name has max length if provided
        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MaximumLength(200)
                .WithMessage("Project name must not exceed 200 characters");
        });

        // Description has max length if provided
        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Project description must not exceed 2000 characters");
        });

        // If Status is provided, validate it
        When(x => x.Status.HasValue, () =>
        {
            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid project status");
        });

        // If both dates provided, DueDate must be after StartDate
        When(x => x.StartDate.HasValue && x.DueDate.HasValue, () =>
        {
            RuleFor(x => x.DueDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("Due date must be after start date");
        });
    }
}
