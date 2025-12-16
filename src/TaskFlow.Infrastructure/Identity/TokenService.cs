using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Identity;

/// <summary>
/// Service for generating and validating JWT tokens.
/// Uses configuration from appsettings.json for JWT settings.
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    /// <summary>
    /// Constructor - loads JWT configuration from appsettings.json.
    /// </summary>
    /// <param name="configuration">Configuration to read JWT settings</param>
    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        // Read JWT settings from appsettings.json
        _secretKey = _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = _configuration["JwtSettings:Issuer"] 
            ?? throw new InvalidOperationException("JWT Issuer not configured");
        _audience = _configuration["JwtSettings:Audience"] 
            ?? throw new InvalidOperationException("JWT Audience not configured");
        _expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");
    }

    /// <summary>
    /// Generates a JWT access token containing user information.
    /// The token includes claims for user ID, email, and name.
    /// </summary>
    public string GenerateAccessToken(User user)
    {
        // Create claims (user information embedded in the token)
        var claims = new[]
        {
            // Standard JWT claims
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),      // Subject (User ID)
            new Claim(JwtRegisteredClaimNames.Email, user.Email),            // Email
            new Claim(JwtRegisteredClaimNames.Name, user.FullName),          // Full name
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID (unique identifier)
            
            // Custom claims (optional - can add role, permissions, etc.)
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };

        // Create signing key from secret
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create the JWT token
        var token = new JwtSecurityToken(
            issuer: _issuer,                                    // Who issued the token
            audience: _audience,                                // Who the token is for
            claims: claims,                                     // User information
            notBefore: DateTime.UtcNow,                        // Token valid from now
            expires: DateTime.UtcNow.AddMinutes(_expiryMinutes), // Token expires after X minutes
            signingCredentials: credentials                     // Signature to prevent tampering
        );

        // Convert token to string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a cryptographically secure random refresh token.
    /// This is a simple random string, not a JWT.
    /// </summary>
    public string GenerateRefreshToken()
    {
        // Create 64 random bytes
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        // Convert to base64 string (safe for URLs and storage)
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Validates a JWT token and extracts the user ID.
    /// Returns null if token is invalid, expired, or tampered with.
    /// </summary>
    public Guid? ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            // Validate the token
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,           // Check signature is valid
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,                     // Check issuer matches
                ValidIssuer = _issuer,
                ValidateAudience = true,                   // Check audience matches
                ValidAudience = _audience,
                ValidateLifetime = true,                   // Check token hasn't expired
                ClockSkew = TimeSpan.Zero                  // No grace period for expiration
            }, out SecurityToken validatedToken);

            // Extract user ID from claims
            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;

            return Guid.Parse(userIdClaim);
        }
        catch (SecurityTokenExpiredException)
        {
            // Token has expired
            return null;
        }
        catch (Exception)
        {
            // Token is invalid for any other reason
            // (wrong signature, malformed, etc.)
            return null;
        }
    }

    /// <summary>
    /// Gets the token expiration time in minutes.
    /// </summary>
    public int GetTokenExpirationMinutes()
    {
        return _expiryMinutes;
    }
}

// ============================================
// JWT Token Structure Explained
// ============================================
//
// A JWT has 3 parts: Header.Payload.Signature
//
// Example JWT:
// eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
//
// Part 1 - Header (base64 encoded):
// {
//   "alg": "HS256",    // Algorithm: HMAC SHA-256
//   "typ": "JWT"       // Type: JWT
// }
//
// Part 2 - Payload (base64 encoded):
// {
//   "sub": "user-id-guid",           // User ID
//   "email": "john@example.com",     // Email
//   "name": "John Doe",              // Full name
//   "exp": 1735920000,               // Expiration timestamp
//   "jti": "unique-id"               // JWT ID
// }
//
// Part 3 - Signature:
// HMACSHA256(
//   base64UrlEncode(header) + "." + base64UrlEncode(payload),
//   secretKey
// )
//
// The signature ensures:
// - Token hasn't been tampered with
// - Token was issued by your server
// - If anyone changes the payload, signature becomes invalid
//
// ============================================
// Security Features
// ============================================
//
// 1. Signing with Secret Key:
//    - Only your server knows the secret key
//    - Can't create valid tokens without it
//    - Prevents token forgery
//
// 2. Signature Validation:
//    - Every request validates the signature
//    - If payload changed, signature won't match
//    - Detects tampering attempts
//
// 3. Expiration (exp claim):
//    - Token automatically expires
//    - Limits damage if token is stolen
//    - Forces periodic re-authentication
//
// 4. Issuer/Audience Validation:
//    - Ensures token is for YOUR application
//    - Prevents token reuse from other apps
//    - Additional layer of validation
//
// ============================================
// Configuration in appsettings.json
// ============================================
//
// {
//   "JwtSettings": {
//     "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
//     "Issuer": "TaskFlowAPI",
//     "Audience": "TaskFlowClient",
//     "ExpiryMinutes": 60,
//     "RefreshTokenExpiryDays": 7
//   }
// }
//
// SecretKey: Secret used to sign tokens (must be long and random!)
// Issuer: Who creates the token (your API)
// Audience: Who the token is for (your frontend)
// ExpiryMinutes: How long access token is valid (15-60 minutes typical)
// RefreshTokenExpiryDays: How long refresh token is valid (7-30 days typical)
//
// ============================================
// Token Validation Flow
// ============================================
//
// 1. Client sends request with token:
//    Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
//
// 2. ASP.NET Core middleware extracts token
//
// 3. ValidateToken() is called:
//    - Check signature matches (not tampered)
//    - Check issuer matches (from your server)
//    - Check audience matches (for your app)
//    - Check not expired (still valid time-wise)
//
// 4a. If valid:
//     - Extract claims (user ID, email, name)
//     - Create ClaimsPrincipal
//     - Make available via HttpContext.User
//     - Request proceeds normally
//
// 4b. If invalid:
//     - Return 401 Unauthorized
//     - Request is rejected
//
// ============================================
// Refresh Token vs Access Token
// ============================================
//
// Access Token (JWT):
// - Contains user information (claims)
// - Short-lived (60 minutes)
// - Sent with every API request
// - Cannot be revoked once issued
// - Stateless (server doesn't store it)
//
// Refresh Token (Random String):
// - Contains no user information
// - Long-lived (7 days)
// - Only used to get new access token
// - Can be revoked (stored in database)
// - Stateful (server tracks it)
//
// Why both?
// - Short-lived access token = secure (stolen token expires quickly)
// - Long-lived refresh token = convenient (user stays logged in)
// - Best of both worlds!
