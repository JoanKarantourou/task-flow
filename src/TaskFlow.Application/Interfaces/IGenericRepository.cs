using System.Linq.Expressions;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Generic repository interface that defines common CRUD operations for all entities.
/// Uses generic type parameter T to work with any entity type.
/// All repositories will implement this interface for consistent data access patterns.
/// </summary>
/// <typeparam name="T">The entity type (User, Project, TaskItem, etc.)</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// Returns null if entity doesn't exist.
    /// </summary>
    /// <param name="id">The unique identifier of the entity</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The entity if found, null otherwise</returns>
    /// <example>
    /// var user = await repository.GetByIdAsync(userId);
    /// </example>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities of type T from the database.
    /// Use with caution on large tables - consider pagination or filtering instead.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of all entities</returns>
    /// <example>
    /// var allUsers = await repository.GetAllAsync();
    /// </example>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves entities that match the specified predicate (filter condition).
    /// Allows flexible querying with LINQ expressions.
    /// </summary>
    /// <param name="predicate">Lambda expression defining the filter condition</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of entities matching the condition</returns>
    /// <example>
    /// // Find all active projects
    /// var activeProjects = await repository.FindAsync(p => p.Status == ProjectStatus.Active);
    /// 
    /// // Find tasks due this week
    /// var dueTasks = await repository.FindAsync(t => t.DueDate <= DateTime.UtcNow.AddDays(7));
    /// </example>
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single entity matching the predicate.
    /// Returns null if no match found.
    /// Throws exception if multiple matches found.
    /// </summary>
    /// <param name="predicate">Lambda expression defining the filter condition</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Single entity if found, null otherwise</returns>
    /// <example>
    /// // Find user by email
    /// var user = await repository.SingleOrDefaultAsync(u => u.Email == "john@example.com");
    /// </example>
    Task<T?> SingleOrDefaultAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the specified predicate.
    /// More efficient than retrieving the entity when you only need existence check.
    /// </summary>
    /// <param name="predicate">Lambda expression defining the condition to check</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>True if at least one entity matches, false otherwise</returns>
    /// <example>
    /// // Check if email is already taken
    /// var emailExists = await repository.AnyAsync(u => u.Email == newEmail);
    /// if (emailExists) throw new Exception("Email already registered");
    /// </example>
    Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the database.
    /// The entity is not saved until SaveChangesAsync (via UnitOfWork) is called.
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The added entity (with generated Id)</returns>
    /// <example>
    /// var newUser = new User { Email = "john@example.com", ... };
    /// await repository.AddAsync(newUser);
    /// await unitOfWork.SaveChangesAsync(); // Actually saves to database
    /// </example>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities in a single operation.
    /// More efficient than calling AddAsync multiple times.
    /// Changes are not saved until SaveChangesAsync is called.
    /// </summary>
    /// <param name="entities">Collection of entities to add</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <example>
    /// var tasks = new List&lt;TaskItem&gt; { task1, task2, task3 };
    /// await repository.AddRangeAsync(tasks);
    /// await unitOfWork.SaveChangesAsync();
    /// </example>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an entity as modified in the change tracker.
    /// The entity is not updated in database until SaveChangesAsync is called.
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <example>
    /// var user = await repository.GetByIdAsync(userId);
    /// user.Email = "newemail@example.com";
    /// repository.Update(user);
    /// await unitOfWork.SaveChangesAsync(); // Actually updates database
    /// </example>
    void Update(T entity);

    /// <summary>
    /// Updates multiple entities in a single operation.
    /// More efficient than calling Update multiple times.
    /// Changes are not saved until SaveChangesAsync is called.
    /// </summary>
    /// <param name="entities">Collection of entities to update</param>
    /// <example>
    /// foreach (var task in tasks)
    /// {
    ///     task.Status = TaskStatus.Completed;
    /// }
    /// repository.UpdateRange(tasks);
    /// await unitOfWork.SaveChangesAsync();
    /// </example>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// Marks an entity for deletion.
    /// The entity is not removed from database until SaveChangesAsync is called.
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    /// <example>
    /// var task = await repository.GetByIdAsync(taskId);
    /// repository.Delete(task);
    /// await unitOfWork.SaveChangesAsync(); // Actually deletes from database
    /// </example>
    void Delete(T entity);

    /// <summary>
    /// Deletes multiple entities in a single operation.
    /// More efficient than calling Delete multiple times.
    /// Changes are not saved until SaveChangesAsync is called.
    /// </summary>
    /// <param name="entities">Collection of entities to delete</param>
    /// <example>
    /// var oldTasks = await repository.FindAsync(t => t.CreatedAt < cutoffDate);
    /// repository.DeleteRange(oldTasks);
    /// await unitOfWork.SaveChangesAsync();
    /// </example>
    void DeleteRange(IEnumerable<T> entities);

    /// <summary>
    /// Gets the total count of entities matching the predicate.
    /// If no predicate provided, returns total count of all entities.
    /// </summary>
    /// <param name="predicate">Optional filter condition</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Number of entities matching the condition</returns>
    /// <example>
    /// // Count all users
    /// var totalUsers = await repository.CountAsync();
    /// 
    /// // Count active projects
    /// var activeCount = await repository.CountAsync(p => p.Status == ProjectStatus.Active);
    /// </example>
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null, 
        CancellationToken cancellationToken = default);
}
