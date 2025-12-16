using FluentValidation;

namespace TaskFlow.Application.Features.Auth.Commands.Login;

/// <summary>
/// Validator for LoginCommand.
/// Validates login credentials before processing.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// Constructor - defines validation rules.
    /// </summary>
    public LoginCommandValidator()
    {
        // Email validation
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");

        // Password validation
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required");
    }
}

// ============================================
// Login Validation vs Registration
// ============================================
//
// Login validation is SIMPLER than registration:
// - Only check email format (not uniqueness)
// - Only check password not empty (not complexity)
//
// Why?
// - User already registered with valid data
// - Just checking credentials exist
// - Actual verification done in AuthService
//
// We don't validate:
// - Password complexity (already enforced at registration)
// - Email uniqueness (will check in service)
// - Password match (no confirmation field)
//
// Keep validation minimal for login!
// Business logic validation happens in AuthService.
