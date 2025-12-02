namespace TaskFlow.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a project.
/// Projects move through these states from creation to completion.
/// </summary>
public enum ProjectStatus
{
    /// <summary>
    /// Project is currently active and work is ongoing.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Project is temporarily paused but may resume later.
    /// </summary>
    OnHold = 1,

    /// <summary>
    /// Project has been successfully completed.
    /// All tasks are done and objectives met.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Project is archived and no longer active.
    /// Used for long-term storage of old projects.
    /// </summary>
    Archived = 3
}