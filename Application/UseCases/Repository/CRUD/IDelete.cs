using Application.Result;
using Domain.Interfaces.Entity;

namespace Application.UseCases.Repository.CRUD
{
    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public interface IDelete<T> where T : class, IEntity
    {
        /// <summary>
        /// Remove the entity with the specified ID.
        /// </summary>
        /// <param name="id">Unique identifier of the entity.</param>
        /// <returns>
        /// An <see cref="Operation{bool}"/> indicating if the delete succeeded.
        /// </returns>
        Task<Operation<bool>> DeleteEntity(string id);
    }
}
