using FluentValidation;

namespace TaskFlow.Application.Features.Users.Commands.UpdateProfile;

/// <summary>
/// Validator for UpdateProfileCommand.
/// </summary>
public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        // First name validation if provided
        When(x => !string.IsNullOrWhiteSpace(x.FirstName), () =>
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(100)
                .WithMessage("First name must not exceed 100 characters");
        });

        // Last name validation if provided
        When(x => !string.IsNullOrWhiteSpace(x.LastName), () =>
        {
            RuleFor(x => x.LastName)
                .MaximumLength(100)
                .WithMessage("Last name must not exceed 100 characters");
        });

        // Email validation if provided
        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Invalid email address format")
                .MaximumLength(256)
                .WithMessage("Email must not exceed 256 characters");
        });
    }
}
