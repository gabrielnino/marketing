using Domain.Interfaces.Entity;
using Persistence.Context.Interface;
using System.Linq.Expressions;

namespace Persistence.Repositories
{
    /// <summary>
    /// Base class for read-only queries with filtering.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public abstract class Read<T>(IUnitOfWork unitOfWork) : Repository<T>(unitOfWork) where T : class, IEntity
    {
        /// <summary>
        /// Retrieves entities that match the given condition.
        /// </summary>
        /// <param name="predicate">Expression to filter the entities.</param>
        /// <returns>
        /// A task containing an IQueryable of matching entities.
        /// </returns>
        protected Task<IQueryable<T>> ReadFilter(Expression<Func<T, bool>> predicate)
        {
            return Task.FromResult(_dbSet.Where(predicate));
        }
    }
}
