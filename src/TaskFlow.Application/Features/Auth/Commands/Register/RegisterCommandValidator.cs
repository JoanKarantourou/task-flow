using FluentValidation;

namespace TaskFlow.Application.Features.Auth.Commands.Register;

/// <summary>
/// Validator for RegisterCommand.
/// Validates registration data before processing.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>
    /// Constructor - defines validation rules.
    /// </summary>
    public RegisterCommandValidator()
    {
        // Email validation
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(100)
            .WithMessage("Email must not exceed 100 characters");

        // Password validation
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .MaximumLength(100)
            .WithMessage("Password must not exceed 100 characters")
            .Matches(@"[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
            .WithMessage("Password must contain at least one number")
            .Matches(@"[\!\@\#\$\%\^\&\*\(\)_\+\-\=\[\]\{\}\;\:\'\,\.\<\>\?\/\\|`~]")
            .WithMessage("Password must contain at least one special character");

        // Confirm password validation
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Password confirmation is required")
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match");

        // First name validation
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(50)
            .WithMessage("First name must not exceed 50 characters")
            .Matches(@"^[a-zA-Z\s\-]+$")
            .WithMessage("First name can only contain letters, spaces, and hyphens");

        // Last name validation
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(50)
            .WithMessage("Last name must not exceed 50 characters")
            .Matches(@"^[a-zA-Z\s\-]+$")
            .WithMessage("Last name can only contain letters, spaces, and hyphens");
    }
}

// ============================================
// FluentValidation Explained
// ============================================
//
// RuleFor(x => x.PropertyName)
// - Defines validation rules for a property
// - Returns RuleBuilder for method chaining
//
// Common Validators:
// - NotEmpty() - Not null, empty, or whitespace
// - EmailAddress() - Valid email format
// - MaximumLength(n) - String length <= n
// - MinimumLength(n) - String length >= n
// - Matches(regex) - Matches regular expression
// - Equal(value) - Equals specified value
//
// WithMessage(msg):
// - Custom error message
// - Shown when validation fails
//
// ============================================
// Password Requirements
// ============================================
//
// Our password policy:
// - At least 8 characters
// - At least one uppercase letter (A-Z)
// - At least one lowercase letter (a-z)
// - At least one number (0-9)
// - At least one special character (!@#$%^&*...)
//
// This is a reasonable security baseline.
// You can adjust based on requirements.
//
// ============================================
// Validation Pipeline
// ============================================
//
// Request Flow:
// 1. Controller receives RegisterCommand
// 2. MediatR sends to pipeline
// 3. ValidationBehavior intercepts
// 4. Runs RegisterCommandValidator
// 5a. If valid: Proceeds to handler
// 5b. If invalid: Throws ValidationException
// 6. Exception middleware catches it
// 7. Returns 400 Bad Request with errors
//
// Example error response:
// {
//   "errors": {
//     "Email": ["Email is required"],
//     "Password": [
//       "Password must be at least 8 characters long",
//       "Password must contain at least one uppercase letter"
//     ]
//   }
// }
//
// ============================================
// Validation vs Business Rules
// ============================================
//
// Validator checks:
// - Data format is correct
// - Required fields present
// - String lengths within limits
// - Data types match
//
// Handler/Service checks:
// - Business logic rules
// - Database constraints
// - User permissions
// - Data relationships
//
// Example:
// - Validator: Email format valid? ✓
// - Service: Email already registered? ✓
//
// Both are important!
// Validator = Input validation
// Service = Business validation
