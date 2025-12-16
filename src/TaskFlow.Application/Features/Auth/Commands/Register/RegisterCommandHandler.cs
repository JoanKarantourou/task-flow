using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handler for RegisterCommand.
/// Uses AuthService to create a new user account and generate tokens.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, TokenDto>
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Constructor - DI injects dependencies.
    /// </summary>
    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Handles the registration command.
    /// Creates a new user account and returns authentication tokens.
    /// </summary>
    public async Task<TokenDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Create RegisterDto from command
        var registerDto = new RegisterDto
        {
            Email = request.Email,
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        // Call AuthService to register user
        // This will:
        // 1. Validate passwords match
        // 2. Check email is unique
        // 3. Hash the password
        // 4. Create user in database
        // 5. Generate access + refresh tokens
        // 6. Return TokenDto
        var tokenDto = await _authService.RegisterAsync(registerDto, cancellationToken);

        return tokenDto;
    }
}

// ============================================
// Registration Flow
// ============================================
//
// Controller → MediatR.Send(RegisterCommand)
//     ↓
// ValidationBehavior validates command
//     ↓
// RegisterCommandHandler.Handle()
//     ↓
// AuthService.RegisterAsync()
//     ↓
// - Check email unique
// - Hash password with BCrypt
// - Create User entity
// - Generate JWT access token
// - Generate refresh token
// - Save to database
//     ↓
// Return TokenDto { AccessToken, RefreshToken, User }
//     ↓
// Controller returns 200 OK with tokens
//
// ============================================
// Error Handling
// ============================================
//
// Errors thrown by AuthService:
// - InvalidOperationException: "Email is already registered"
// - ArgumentException: "Passwords do not match"
//
// These will be caught by exception handling middleware
// and converted to appropriate HTTP responses:
// - 400 Bad Request for validation errors
// - 409 Conflict for duplicate email
