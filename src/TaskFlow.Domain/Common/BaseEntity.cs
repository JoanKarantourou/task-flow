namespace TaskFlow.Domain.Common;

/// <summary>
/// Base entity class that provides common properties for all domain entities.
/// All entities inherit from this to ensure consistent auditing and identification.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// Using Guid ensures globally unique IDs across distributed systems.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Timestamp when the entity was created.
    /// Automatically set when entity is first added to the database.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the entity was last modified.
    /// Updated automatically whenever the entity is changed.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Constructor initializes Id with a new Guid and sets creation timestamp.
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}