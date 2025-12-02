using Microsoft.EntityFrameworkCore.Storage;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation that manages database transactions.
/// Coordinates the work of multiple repositories and ensures all changes
/// are committed or rolled back together as a single transaction.
/// Implements the Unit of Work pattern for data consistency.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy initialization of repositories
    // Only created when first accessed
    private IProjectRepository? _projects;
    private ITaskRepository? _tasks;

    /// <summary>
    /// Constructor - receives DbContext from DI.
    /// </summary>
    /// <param name="context">Application database context</param>
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the Project repository.
    /// Lazy initialization - created only when first accessed.
    /// </summary>
    public IProjectRepository Projects
    {
        get
        {
            // If repository doesn't exist yet, create it
            _projects ??= new ProjectRepository(_context);
            return _projects;
        }
    }

    /// <summary>
    /// Gets the Task repository.
    /// Lazy initialization - created only when first accessed.
    /// </summary>
    public ITaskRepository Tasks
    {
        get
        {
            // If repository doesn't exist yet, create it
            _tasks ??= new TaskRepository(_context);
            return _tasks;
        }
    }

    /// <summary>
    /// Saves all pending changes to the database as a single transaction.
    /// If any operation fails, all changes are rolled back.
    /// Returns the number of state entries written to the database.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begins a new database transaction explicitly.
    /// Useful when you need fine-grained control over transaction boundaries.
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // Only start transaction if one isn't already active
        _transaction ??= await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Commits the current transaction, making all changes permanent.
    /// Should be called after BeginTransactionAsync when all operations succeed.
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Save any pending changes
            await _context.SaveChangesAsync(cancellationToken);

            // Commit the transaction
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            // If commit fails, rollback
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            // Cleanup transaction
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    /// <summary>
    /// Rolls back the current transaction, discarding all changes.
    /// Should be called after BeginTransactionAsync if any operation fails.
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            // Cleanup transaction
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    /// <summary>
    /// Disposes the UnitOfWork and its resources.
    /// Called automatically at the end of each request (Scoped lifetime).
    /// </summary>
    public void Dispose()
    {
        // Dispose transaction if it exists
        _transaction?.Dispose();

        // Note: We don't dispose _context here because it's managed by DI
        // The DI container will dispose it at the end of the request
    }
}