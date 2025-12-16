using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handler for LoginCommand.
/// Uses AuthService to authenticate user and generate tokens.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenDto>
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Constructor - DI injects dependencies.
    /// </summary>
    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Handles the login command.
    /// Authenticates user and returns tokens.
    /// </summary>
    public async Task<TokenDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Create LoginDto from command
        var loginDto = new LoginDto
        {
            Email = request.Email,
            Password = request.Password
        };

        // Call AuthService to authenticate user
        // This will:
        // 1. Find user by email
        // 2. Verify password hash
        // 3. Generate access + refresh tokens
        // 4. Update refresh token in database
        // 5. Return TokenDto
        var tokenDto = await _authService.LoginAsync(loginDto, cancellationToken);

        return tokenDto;
    }
}

// ============================================
// Login Flow
// ============================================
//
// Controller → MediatR.Send(LoginCommand)
//     ↓
// ValidationBehavior validates command
//     ↓
// LoginCommandHandler.Handle()
//     ↓
// AuthService.LoginAsync()
//     ↓
// - Find user by email
// - Verify password with BCrypt.Verify()
// - Generate JWT access token
// - Generate refresh token
// - Update user's refresh token in DB
//     ↓
// Return TokenDto { AccessToken, RefreshToken, User }
//     ↓
// Controller returns 200 OK with tokens
//
// ============================================
// Security Considerations
// ============================================
//
// 1. Don't reveal which credential is wrong:
//    - Wrong email: "Invalid credentials"
//    - Wrong password: "Invalid credentials"
//    - Never say "email not found" or "wrong password"
//    - Prevents email enumeration attacks
//
// 2. Password verification:
//    - Uses BCrypt.Verify() (constant-time comparison)
//    - Prevents timing attacks
//
// 3. Token generation:
//    - New tokens on each login
//    - Old refresh token is invalidated
//    - Prevents token reuse
//
// ============================================
// Error Handling
// ============================================
//
// AuthService throws:
// - UnauthorizedAccessException: "Invalid credentials"
//   → Caught by middleware
//   → Returns 401 Unauthorized
//
// ValidationBehavior throws:
// - ValidationException: Email/password format invalid
//   → Caught by middleware
//   → Returns 400 Bad Request
