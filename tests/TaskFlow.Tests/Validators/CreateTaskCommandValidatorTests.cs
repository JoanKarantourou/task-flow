using FluentValidation.TestHelper;
using TaskFlow.Application.Features.Tasks.Commands.CreateTask;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Tests.Validators;

public class CreateTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _validator = new();

    private static CreateTaskCommand ValidCommand() => new()
    {
        Title = "Implement feature X",
        Description = "Some description",
        ProjectId = Guid.NewGuid(),
        Priority = TaskPriority.Medium,
        DueDate = DateTime.UtcNow.AddDays(7)
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
    public void Should_Fail_When_Title_Empty(string? title)
    {
        var cmd = ValidCommand();
        cmd.Title = title!;
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Fail_When_Title_Exceeds_MaxLength()
    {
        var cmd = ValidCommand();
        cmd.Title = new string('a', 201);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Fail_When_Title_Contains_Script_Tag()
    {
        var cmd = ValidCommand();
        cmd.Title = "<script>alert('xss')</script>";
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Fail_When_ProjectId_Empty()
    {
        var cmd = ValidCommand();
        cmd.ProjectId = Guid.Empty;
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ProjectId);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_MaxLength()
    {
        var cmd = ValidCommand();
        cmd.Description = new string('a', 2001);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_DueDate_In_Past()
    {
        var cmd = ValidCommand();
        cmd.DueDate = DateTime.UtcNow.AddDays(-1);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public void Should_Pass_When_DueDate_Null()
    {
        var cmd = ValidCommand();
        cmd.DueDate = null;
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public void Should_Fail_When_AssigneeId_Is_Empty_Guid()
    {
        var cmd = ValidCommand();
        cmd.AssigneeId = Guid.Empty;
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.AssigneeId);
    }

    [Fact]
    public void Should_Pass_When_AssigneeId_Null()
    {
        var cmd = ValidCommand();
        cmd.AssigneeId = null;
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.AssigneeId);
    }
}
