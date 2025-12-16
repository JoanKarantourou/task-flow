using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;

namespace TaskFlow.Infrastructure.Identity;

/// <summary>
/// Service for handling user authentication operations.
/// Manages registration, login, and token refresh functionality.
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly int _refreshTokenExpiryDays;

    /// <summary>
    /// Constructor - DI injects dependencies.
    /// </summary>
    public AuthService(
        ApplicationDbContext context,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _context = context;
        _tokenService = tokenService;
        _configuration = configuration;
        _refreshTokenExpiryDays = int.Parse(
            _configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    public async Task<TokenDto> RegisterAsync(
        RegisterDto registerDto, 
        CancellationToken cancellationToken = default)
    {
        // Step 1: Validate passwords match
        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            throw new ArgumentException("Passwords do not match");
        }

        // Step 2: Check if email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == registerDto.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email is already registered");
        }

        // Step 3: Hash the password with BCrypt
        // BCrypt automatically generates a salt and includes it in the hash
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        // Step 4: Create the user entity
        var user = new User
        {
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PasswordHash = passwordHash
            // Id, CreatedAt, UpdatedAt are set automatically by BaseEntity
        };

        // Step 5: Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Step 6: Store refresh token in user record
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        // Step 7: Save user to database
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Step 8: Return tokens and user info
        return new TokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetTokenExpirationMinutes()),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt
            }
        };
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    public async Task<TokenDto> LoginAsync(
        LoginDto loginDto, 
        CancellationToken cancellationToken = default)
    {
        // Step 1: Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email, cancellationToken);

        // Step 2: Check if user exists and password is correct
        // IMPORTANT: Don't reveal whether email exists or password is wrong
        // Always return generic "Invalid credentials" message
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Step 3: Generate new tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Step 4: Update refresh token in database
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        await _context.SaveChangesAsync(cancellationToken);

        // Step 5: Return tokens and user info
        return new TokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetTokenExpirationMinutes()),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt
            }
        };
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// Implements refresh token rotation for security.
    /// </summary>
    public async Task<TokenDto> RefreshTokenAsync(
        RefreshTokenDto refreshTokenDto, 
        CancellationToken cancellationToken = default)
    {
        // Step 1: Validate the access token (even though expired, we need user ID)
        // We use ValidateToken which will return null for expired tokens,
        // but we can extract user ID from the token manually
        var userId = ExtractUserIdFromExpiredToken(refreshTokenDto.AccessToken);
        
        if (userId == null)
        {
            throw new UnauthorizedAccessException("Invalid access token");
        }

        // Step 2: Find user by ID
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Step 3: Validate refresh token
        if (user.RefreshToken != refreshTokenDto.RefreshToken)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Step 4: Check refresh token hasn't expired
        if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token has expired");
        }

        // Step 5: Generate NEW tokens (token rotation)
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Step 6: Update user with NEW refresh token (invalidate old one)
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        await _context.SaveChangesAsync(cancellationToken);

        // Step 7: Return NEW tokens
        return new TokenDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetTokenExpirationMinutes()),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt
            }
        };
    }

    /// <summary>
    /// Extracts user ID from an expired JWT token.
    /// We need this because ValidateToken returns null for expired tokens,
    /// but we still need the user ID to look up the refresh token.
    /// </summary>
    private Guid? ExtractUserIdFromExpiredToken(string token)
    {
        try
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Extract user ID from "sub" claim
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return null;
            }

            return Guid.Parse(userIdClaim);
        }
        catch
        {
            return null;
        }
    }
}

// ============================================
// BCrypt Password Hashing Explained
// ============================================
//
// BCrypt.HashPassword():
// - Generates a random salt
// - Combines salt with password
// - Hashes the combination multiple times (work factor)
// - Returns: $2a$10$salt+hash (includes salt in result)
//
// Example output:
// $2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy
//  ^   ^   ^----- 22 char salt ----^-------- 31 char hash ----------^
//  |   |
//  |   +-- Work factor (10 = 2^10 = 1024 iterations)
//  +------ BCrypt version
//
// BCrypt.Verify():
// - Extracts salt from stored hash
// - Hashes provided password with that salt
// - Compares result with stored hash
// - Returns true if they match
//
// Why BCrypt?
// - Slow by design (prevents brute force)
// - Includes salt (prevents rainbow table attacks)
// - Adjustable work factor (can increase security over time)
// - Industry standard
//
// ============================================
// Security Best Practices
// ============================================
//
// 1. Never Store Plain Text Passwords:
//    - Always hash with BCrypt (or Argon2, PBKDF2)
//    - Store only the hash, never the original password
//    - Even admins can't see user passwords
//
// 2. Don't Reveal Which Credential is Wrong:
//    - Wrong email: "Invalid credentials"
//    - Wrong password: "Invalid credentials"
//    - This prevents email enumeration attacks
//
// 3. Refresh Token Rotation:
//    - Generate NEW refresh token on each use
//    - Invalidate OLD refresh token
//    - Prevents token replay attacks
//    - Detects token theft (old token used = alert!)
//
// 4. Token Storage:
//    - Database stores: RefreshToken + ExpiryTime
//    - Can be revoked if compromised
//    - Can track usage patterns
//    - Enables "logout everywhere" functionality
//
// ============================================
// Authentication Flow Diagrams
// ============================================
//
// REGISTRATION FLOW:
// User → Email/Password → AuthService.Register()
//   ↓
// Check email unique
//   ↓
// Hash password with BCrypt
//   ↓
// Create User entity
//   ↓
// Generate access token (JWT)
//   ↓
// Generate refresh token (random)
//   ↓
// Save user + refresh token to DB
//   ↓
// Return TokenDto to client
//
// LOGIN FLOW:
// User → Email/Password → AuthService.Login()
//   ↓
// Find user by email
//   ↓
// Verify password with BCrypt.Verify()
//   ↓
// Generate NEW access token
//   ↓
// Generate NEW refresh token
//   ↓
// Update user's refresh token in DB
//   ↓
// Return TokenDto to client
//
// REFRESH TOKEN FLOW:
// Client → Expired Access Token + Refresh Token → AuthService.RefreshToken()
//   ↓
// Extract user ID from expired token
//   ↓
// Find user in database
//   ↓
// Validate refresh token matches
//   ↓
// Check refresh token not expired
//   ↓
// Generate NEW access token
//   ↓
// Generate NEW refresh token (rotation!)
//   ↓
// Invalidate OLD refresh token in DB
//   ↓
// Return NEW TokenDto to client
//
// ============================================
// Error Handling
// ============================================
//
// Registration Errors:
// - Email exists: "Email is already registered"
// - Passwords don't match: "Passwords do not match"
// - Invalid email format: Caught by FluentValidation
//
// Login Errors:
// - Wrong email OR password: "Invalid credentials"
// - Don't say which one is wrong (security!)
//
// Refresh Token Errors:
// - Invalid token: "Invalid refresh token"
// - Expired token: "Refresh token has expired"
// - Token already used: Detected by mismatch
// - User not found: "User not found"
//
// ============================================
// Future Enhancements
// ============================================
//
// 1. Account Lockout:
//    - Track failed login attempts
//    - Lock account after X failures
//    - Unlock after time period or admin action
//
// 2. Email Verification:
//    - Send verification email on registration
//    - Don't allow login until verified
//    - Prevents fake email registrations
//
// 3. Two-Factor Authentication (2FA):
//    - Require second factor (SMS, authenticator app)
//    - Add 2FA token validation to login flow
//    - Store 2FA secret in User entity
//
// 4. Password Reset:
//    - Generate reset token
//    - Send email with reset link
//    - Validate token and update password
//
// 5. Session Management:
//    - Store refresh tokens in separate table
//    - Track device/IP/location
//    - Allow user to see and revoke active sessions
//    - "Logout everywhere" functionality
