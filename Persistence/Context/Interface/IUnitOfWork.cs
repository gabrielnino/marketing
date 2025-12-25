using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Context.Implementation;

namespace Persistence.Context.Interface
{
    /// <summary>
    /// Defines methods to manage database changes and transactions.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Persists all pending changes to the database.
        /// </summary>
        Task<int> CommitAsync();

        /// <summary>
        /// Starts a new database transaction.
        /// </summary>
        Task<IDbContextTransaction> BeginTransactionAsync();

        /// <summary>
        /// Commits the specified transaction after saving changes.
        /// </summary>
        Task CommitTransactionAsync(IDbContextTransaction tx);

        /// <summary>
        /// Rolls back the specified transaction.
        /// </summary>
        Task RollbackAsync(IDbContextTransaction tx);

        /// <summary>
        /// The underlying data context.
        /// </summary>
        DataContext Context { get; }
    }
}
