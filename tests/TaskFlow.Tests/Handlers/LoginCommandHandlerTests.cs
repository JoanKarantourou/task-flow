using FluentAssertions;
using Moq;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Features.Auth.Commands.Login;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Tests.Handlers;

public class LoginCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokenDto()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "SecurePass1!"
        };

        var expectedToken = new TokenDto
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                FullName = "John Doe"
            }
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(It.Is<LoginDto>(d =>
                d.Email == command.Email &&
                d.Password == command.Password), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "WrongPassword!"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
