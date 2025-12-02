using FluentValidation;

namespace TaskFlow.Application.Features.Tasks.Commands.CreateTask;

/// <summary>
/// Validator for CreateTaskCommand.
/// Uses FluentValidation to define validation rules in a readable, fluent syntax.
/// This validator runs BEFORE the handler executes.
/// If validation fails, the handler never runs and an error is returned to the client.
/// </summary>
/// <remarks>
/// FluentValidation benefits:
/// - Clear, readable validation rules
/// - Separates validation from business logic
/// - Easy to test validation rules independently
/// - Provides detailed, customizable error messages
/// - Can perform complex validation with custom rules
/// </remarks>
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    /// <summary>
    /// Constructor where we define all validation rules.
    /// Rules are evaluated in the order they're defined.
    /// </summary>
    public CreateTaskCommandValidator()
    {
        // Title validation
        // Title is required and must be between 1 and 200 characters
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Task title is required.")
            .MaximumLength(200)
            .WithMessage("Task title cannot exceed 200 characters.")
            .Must(BeValidTitle)
            .WithMessage("Task title contains invalid characters.");

        // Description validation
        // Description is optional but if provided, has a max length
        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Task description cannot exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description)); // Only validate if provided

        // ProjectId validation
        // Must be a valid GUID (not empty)
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        // Priority validation
        // Must be a valid enum value
        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Invalid priority value.");

        // AssigneeId validation
        // Optional, but if provided must be a valid GUID
        RuleFor(x => x.AssigneeId)
            .NotEqual(Guid.Empty)
            .WithMessage("Assignee ID must be a valid GUID.")
            .When(x => x.AssigneeId.HasValue); // Only validate if provided

        // DueDate validation
        // Optional, but if provided should be in the future
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Due date cannot be in the past.")
            .When(x => x.DueDate.HasValue); // Only validate if provided
    }

    /// <summary>
    /// Custom validation method to check if title contains valid characters.
    /// Prevents potential security issues like script injection.
    /// </summary>
    /// <param name="title">The task title to validate</param>
    /// <returns>True if title is valid, false otherwise</returns>
    private bool BeValidTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return false;
        }

        // Check for common injection patterns
        // In a real application, you might want more sophisticated validation
        var invalidPatterns = new[] { "<script", "</script", "javascript:", "onerror=" };

        foreach (var pattern in invalidPatterns)
        {
            if (title.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Alternative: You can also use async validation for database checks
    /// Example: Check if project exists during validation
    /// </summary>
    /// <remarks>
    /// Uncomment this if you want to validate project existence during validation phase:
    /// 
    /// private async Task&lt;bool&gt; ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken)
    /// {
    ///     // Inject IUnitOfWork in constructor and use it here
    ///     return await _unitOfWork.Projects.AnyAsync(p => p.Id == projectId, cancellationToken);
    /// }
    /// 
    /// Then add this rule in constructor:
    /// RuleFor(x => x.ProjectId)
    ///     .MustAsync(ProjectExistsAsync)
    ///     .WithMessage("The specified project does not exist.");
    /// 
    /// Note: This adds a database call during validation.
    /// We do this check in the handler instead to avoid duplicate queries.
    /// </remarks>
}