using FluentValidation;

namespace TaskFlow.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Validator for RefreshTokenCommand.
/// Validates that both tokens are provided.
/// </summary>
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>
    /// Constructor - defines validation rules.
    /// </summary>
    public RefreshTokenCommandValidator()
    {
        // Access token validation
        RuleFor(x => x.AccessToken)
            .NotEmpty()
            .WithMessage("Access token is required");

        // Refresh token validation
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required");
    }
}

// ============================================
// Minimal Validation for Refresh
// ============================================
//
// We only check that tokens are provided.
// We don't validate:
// - Token format (JWT structure)
// - Token expiration
// - Token signature
// - Token claims
//
// Why?
// - Actual validation happens in AuthService
// - We need to extract user ID even from expired token
// - Complex validation would duplicate service logic
//
// The service will handle:
// - Extracting user ID from access token
// - Validating refresh token against database
// - Checking refresh token expiration
// - Verifying tokens match the user
//
// Keep validation simple here!
