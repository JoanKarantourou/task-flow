using FluentAssertions;
using Moq;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Features.Auth.Commands.Register;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Tests.Handlers;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsTokenDto()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "SecurePass1!",
            ConfirmPassword = "SecurePass1!",
            FirstName = "John",
            LastName = "Doe"
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
            .Setup(x => x.RegisterAsync(It.Is<RegisterDto>(d =>
                d.Email == command.Email &&
                d.Password == command.Password &&
                d.FirstName == command.FirstName &&
                d.LastName == command.LastName), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.User.Email.Should().Be("test@example.com");
        _authServiceMock.Verify(x => x.RegisterAsync(It.IsAny<RegisterDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "existing@example.com",
            Password = "SecurePass1!",
            ConfirmPassword = "SecurePass1!",
            FirstName = "John",
            LastName = "Doe"
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Email is already registered"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
