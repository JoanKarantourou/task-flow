using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation that provides common CRUD operations for all entities.
/// Uses Entity Framework Core DbContext for database access.
/// All specific repositories (ProjectRepository, TaskRepository) inherit from this.
/// </summary>
/// <typeparam name="T">The entity type (must be a class)</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// Constructor - receives DbContext from DI.
    /// </summary>
    /// <param name="context">Application database context</param>
    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>(); // Gets the DbSet for entity type T
    }

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// Retrieves all entities of type T from the database.
    /// Use with caution on large tables - consider pagination instead.
    /// </summary>
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves entities that match the specified predicate (filter condition).
    /// </summary>
    /// <example>
    /// var activeProjects = await repository.FindAsync(p => p.Status == ProjectStatus.Active);
    /// </example>
    public async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a single entity matching the predicate.
    /// Returns null if no match found.
    /// Throws exception if multiple matches found.
    /// </summary>
    public async Task<T?> SingleOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Checks if any entity matches the specified predicate.
    /// More efficient than retrieving the entity when you only need existence check.
    /// </summary>
    /// <example>
    /// var emailExists = await repository.AnyAsync(u => u.Email == newEmail);
    /// </example>
    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Adds a new entity to the database.
    /// The entity is not saved until SaveChangesAsync (via UnitOfWork) is called.
    /// </summary>
    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// Adds multiple entities in a single operation.
    /// More efficient than calling AddAsync multiple times.
    /// Changes are not saved until SaveChangesAsync is called.
    /// </summary>
    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Marks an entity as modified in the change tracker.
    /// The entity is not updated in database until SaveChangesAsync is called.
    /// </summary>
    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <summary>
    /// Updates multiple entities in a single operation.
    /// More efficient than calling Update multiple times.
    /// Changes are not saved until SaveChangesAsync is called.
    /// </summary>
    public void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    /// <summary>
    /// Marks an entity for deletion.
    /// The entity is not removed from database until SaveChangesAsync is called.
    /// </summary>
    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Deletes multiple entities in a single operation.
    /// More efficient than calling Delete multiple times.
    /// Changes are not saved until SaveChangesAsync is called.
    /// </summary>
    public void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    /// <summary>
    /// Gets the total count of entities matching the predicate.
    /// If no predicate provided, returns total count of all entities.
    /// </summary>
    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }

        return await _dbSet.CountAsync(predicate, cancellationToken);
    }
}