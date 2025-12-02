namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing database transactions.
/// Coordinates the work of multiple repositories and ensures all changes
/// are committed or rolled back together as a single transaction.
/// This follows the Unit of Work pattern to maintain data consistency.
/// </summary>
/// <remarks>
/// The Unit of Work pattern:
/// - Groups multiple database operations into a single transaction
/// - Tracks all changes made during a business transaction
/// - Commits all changes together or rolls back if any operation fails
/// - Ensures data consistency across multiple repository operations
/// </remarks>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Saves all pending changes to the database as a single transaction.
    /// If any operation fails, all changes are rolled back.
    /// Returns the number of state entries written to the database.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Number of entities affected by the save operation</returns>
    /// <example>
    /// // Create a project and assign tasks in one transaction
    /// var project = new Project { Name = "New Project" };
    /// await projectRepository.AddAsync(project);
    /// 
    /// var task1 = new TaskItem { ProjectId = project.Id, Title = "Task 1" };
    /// var task2 = new TaskItem { ProjectId = project.Id, Title = "Task 2" };
    /// await taskRepository.AddRangeAsync(new[] { task1, task2 });
    /// 
    /// // All changes saved together - if any fails, none are saved
    /// await unitOfWork.SaveChangesAsync();
    /// </example>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction explicitly.
    /// Useful when you need fine-grained control over transaction boundaries.
    /// Remember to commit or rollback the transaction when done.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <example>
    /// await unitOfWork.BeginTransactionAsync();
    /// try
    /// {
    ///     // Perform multiple operations
    ///     await projectRepository.AddAsync(project);
    ///     await taskRepository.AddAsync(task);
    ///     await unitOfWork.SaveChangesAsync();
    ///     
    ///     // Commit if everything succeeded
    ///     await unitOfWork.CommitTransactionAsync();
    /// }
    /// catch
    /// {
    ///     // Rollback if anything failed
    ///     await unitOfWork.RollbackTransactionAsync();
    ///     throw;
    /// }
    /// </example>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction, making all changes permanent.
    /// Should be called after BeginTransactionAsync when all operations succeed.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction, discarding all changes.
    /// Should be called after BeginTransactionAsync if any operation fails.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    // Repository Properties
    // These provide access to specific repositories through the Unit of Work
    // This centralizes repository access and ensures they share the same DbContext

    /// <summary>
    /// Provides access to the Project repository.
    /// Used for project-specific queries and operations.
    /// </summary>
    IProjectRepository Projects { get; }

    /// <summary>
    /// Provides access to the Task repository.
    /// Used for task-specific queries and operations.
    /// </summary>
    ITaskRepository Tasks { get; }

    // Note: We expose specific repositories here rather than all generic repositories
    // This keeps the Unit of Work interface clean and focused on the main entities
    // User and Comment operations can be accessed through their respective generic repositories
    // if needed, or added here later as the application grows
}
