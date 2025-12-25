using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Persistence.Context.Interface;

namespace Persistence.Repositories
{
    /// <summary>
    /// Base class to delete entities from the database.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    public abstract class RepositoryDelete<T>(IUnitOfWork unitOfWork) 
        : EntityChecker<T>(unitOfWork) where T : class, IEntity
    {
        /// <summary>
        /// Remove the given entity and save changes.
        /// </summary>
        /// <param name="entity">The item to delete (must not be null).</param>
        /// <returns>
        /// True if the delete was successful; otherwise false.
        /// </returns>
        protected void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
