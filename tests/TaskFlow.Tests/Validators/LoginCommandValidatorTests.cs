using FluentValidation.TestHelper;
using TaskFlow.Application.Features.Auth.Commands.Login;

namespace TaskFlow.Tests.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_When_All_Fields_Valid()
    {
        var cmd = new LoginCommand { Email = "test@example.com", Password = "password" };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_When_Email_Empty(string? email)
    {
        var cmd = new LoginCommand { Email = email!, Password = "password" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Fail_When_Email_Invalid()
    {
        var cmd = new LoginCommand { Email = "not-email", Password = "password" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_When_Password_Empty(string? password)
    {
        var cmd = new LoginCommand { Email = "test@example.com", Password = password! };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
