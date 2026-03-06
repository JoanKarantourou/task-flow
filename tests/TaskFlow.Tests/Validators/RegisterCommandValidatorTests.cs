using FluentAssertions;
using FluentValidation.TestHelper;
using TaskFlow.Application.Features.Auth.Commands.Register;

namespace TaskFlow.Tests.Validators;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    private static RegisterCommand ValidCommand() => new()
    {
        Email = "test@example.com",
        Password = "SecurePass1!",
        ConfirmPassword = "SecurePass1!",
        FirstName = "John",
        LastName = "Doe"
    };

    [Fact]
    public void Should_Pass_When_All_Fields_Valid()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_When_Email_Empty(string? email)
    {
        var cmd = ValidCommand();
        cmd.Email = email!;
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Fail_When_Email_Invalid_Format()
    {
        var cmd = ValidCommand();
        cmd.Email = "not-an-email";
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Fail_When_Password_Too_Short()
    {
        var cmd = ValidCommand();
        cmd.Password = "Ab1!";
        cmd.ConfirmPassword = "Ab1!";
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Missing_Uppercase()
    {
        var cmd = ValidCommand();
        cmd.Password = "securepass1!";
        cmd.ConfirmPassword = "securepass1!";
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Missing_Lowercase()
    {
        var cmd = ValidCommand();
        cmd.Password = "SECUREPASS1!";
        cmd.ConfirmPassword = "SECUREPASS1!";
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Missing_Number()
    {
        var cmd = ValidCommand();
        cmd.Password = "SecurePass!";
        cmd.ConfirmPassword = "SecurePass!";
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Missing_SpecialChar()
    {
        var cmd = ValidCommand();
        cmd.Password = "SecurePass1";
        cmd.ConfirmPassword = "SecurePass1";
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Passwords_Do_Not_Match()
    {
        var cmd = ValidCommand();
        cmd.ConfirmPassword = "DifferentPass1!";
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_When_FirstName_Empty(string? name)
    {
        var cmd = ValidCommand();
        cmd.FirstName = name!;
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_Fail_When_FirstName_Contains_Numbers()
    {
        var cmd = ValidCommand();
        cmd.FirstName = "John123";
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_When_LastName_Empty(string? name)
    {
        var cmd = ValidCommand();
        cmd.LastName = name!;
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }
}
