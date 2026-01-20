using FluentValidation;

namespace TaskFlow.Application.Features.Projects.Commands.DeleteProject;

/// <summary>
/// Validator for DeleteProjectCommand.
/// </summary>
public class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required");
    }
}
