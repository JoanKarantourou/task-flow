namespace TaskFlow.Domain.Enums;

/// <summary>
/// Represents the current status of a task in the workflow.
/// Tasks typically flow: Todo → InProgress → InReview → Done
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task is created but work hasn't started yet.
    /// </summary>
    Todo = 0,

    /// <summary>
    /// Task is currently being worked on.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Task work is complete and awaiting review/approval.
    /// </summary>
    InReview = 2,

    /// <summary>
    /// Task is completed and approved.
    /// </summary>
    Done = 3,

    /// <summary>
    /// Task was cancelled and won't be completed.
    /// </summary>
    Cancelled = 4
}