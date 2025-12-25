using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Context.Interface;

namespace Persistence.Context.Implementation
{
    /// <summary>
    /// Coordinates operations against the database context and transactions.
    /// </summary>
    public class UnitOfWork(DataContext context) : IUnitOfWork
    {
        private readonly DataContext _context = context;

        /// <inheritdoc />
        public DataContext Context => _context;

        /// <summary>
        /// Saves all pending changes.
        /// </summary>
        public async Task<int> CommitAsync()
            => await Context.SaveChangesAsync();

        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        public async Task<IDbContextTransaction> BeginTransactionAsync()
            => await Context.Database.BeginTransactionAsync();

        /// <summary>
        /// Commits the given transaction after saving changes.
        /// </summary>
        public async Task CommitTransactionAsync(IDbContextTransaction tx)
        {
            await Context.SaveChangesAsync();
            await tx.CommitAsync();
        }

        /// <summary>
        /// Rolls back the specified transaction.
        /// </summary>
        public async Task RollbackAsync(IDbContextTransaction tx)
            => await tx.RollbackAsync();

        /// <summary>
        /// Disposes the underlying context.
        /// </summary>
        public void Dispose()
            => Context.Dispose();
    }
}

