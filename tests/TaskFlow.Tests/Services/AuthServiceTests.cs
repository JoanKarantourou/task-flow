using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Identity;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        var configData = new Dictionary<string, string?>
        {
            ["JwtSettings:RefreshTokenExpiryDays"] = "7"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("test-access-token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("test-refresh-token");
        _tokenServiceMock.Setup(x => x.GetTokenExpirationMinutes()).Returns(60);

        _authService = new AuthService(_context, _tokenServiceMock.Object, configuration);
    }

    [Fact]
    public async Task RegisterAsync_ValidData_CreatesUserAndReturnsTokens()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "newuser@example.com",
            Password = "SecurePass1!",
            ConfirmPassword = "SecurePass1!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("test-access-token");
        result.RefreshToken.Should().Be("test-refresh-token");
        result.User.Email.Should().Be("newuser@example.com");
        result.User.FirstName.Should().Be("John");
        result.User.LastName.Should().Be("Doe");

        // Verify user was saved to database
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
        savedUser.Should().NotBeNull();
        savedUser!.PasswordHash.Should().NotBeEmpty();
        savedUser.RefreshToken.Should().Be("test-refresh-token");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange - seed existing user
        _context.Users.Add(new User
        {
            Email = "existing@example.com",
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
        });
        await _context.SaveChangesAsync();

        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "SecurePass1!",
            ConfirmPassword = "SecurePass1!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.RegisterAsync(registerDto));
        ex.Message.Should().Be("Email is already registered");
    }

    [Fact]
    public async Task RegisterAsync_PasswordsMismatch_ThrowsArgumentException()
    {
        var registerDto = new RegisterDto
        {
            Email = "user@example.com",
            Password = "SecurePass1!",
            ConfirmPassword = "DifferentPass1!",
            FirstName = "John",
            LastName = "Doe"
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.RegisterAsync(registerDto));
        ex.Message.Should().Be("Passwords do not match");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        // Arrange - seed user with known password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("SecurePass1!");
        _context.Users.Add(new User
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = passwordHash
        });
        await _context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Email = "user@example.com",
            Password = "SecurePass1!"
        };

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("test-access-token");
        result.User.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _context.Users.Add(new User
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword1!")
        });
        await _context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Email = "user@example.com",
            Password = "WrongPassword1!"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(loginDto));
        ex.Message.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_NonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword1!"
        };

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(loginDto));
        ex.Message.Should().Be("Invalid credentials");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
