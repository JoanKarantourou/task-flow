namespace TaskFlow.Domain.Enums;

/// <summary>
/// Represents the priority level of a task.
/// Used to help teams determine which tasks to work on first.
/// </summary>
public enum TaskPriority
{
    /// <summary>
    /// Low priority task - can be done when time permits.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium priority task - normal priority for standard work.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High priority task - should be addressed soon.
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical priority task - requires immediate attention.
    /// May block other work or have significant business impact.
    /// </summary>
    Critical = 3
}