using Application.Result;
using Domain.Interfaces.Entity;

namespace Application.UseCases.Repository.CRUD
{
    /// <summary>
    /// Updates an existing entity of type T.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IUpdate<T> where T : class, IEntity
    {
        /// <summary>
        /// Update the given entity.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <returns>
        /// An operation result indicating success (true) or failure (false).
        /// </returns>
        Task<Operation<bool>> UpdateEntity(T entity);
    }
}
