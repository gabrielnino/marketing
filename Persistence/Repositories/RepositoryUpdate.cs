using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Persistence.Context.Interface;

namespace Persistence.Repositories
{
    /// <summary>
    /// Handles updating entities in the database.
    /// </summary>
    public abstract class RepositoryUpdate<T>(IUnitOfWork unitOfWork) 
        : EntityChecker<T>(unitOfWork) where T : class, IEntity
    {
        /// <summary>
        /// Marks an entity as modified and saves changes.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>True if the save succeeded; otherwise false.</returns>
        protected void Update(T entity)
        {
            unitOfWork.Context.Entry(entity).State = EntityState.Modified;
        }
    }
}
