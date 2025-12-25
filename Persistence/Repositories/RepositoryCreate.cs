using Domain.Interfaces.Entity;
using Persistence.Context.Interface;

namespace Persistence.Repositories
{
    /// <summary>
    /// Base repository providing async entity creation.
    /// </summary>
    public abstract class RepositoryCreate<T>(IUnitOfWork unitOfWork)
        : Read<T>(unitOfWork) where T : class, IEntity
    {
        /// <summary>
        /// Queues the given entity for insertion.
        /// </summary>
        protected async Task Create(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        protected async Task CreateRange(List<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }
    }
}
