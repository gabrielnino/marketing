using Application.Result;
using Domain.Interfaces.Entity;

namespace Application.UseCases.Repository.CRUD.Query
{
    /// <summary>
    /// Provides a method to retrieve an entity by its ID.
    /// </summary>
    /// <typeparam name="T">Type of entity.</typeparam>
    public interface IReadById<T> where T : class, IEntity
    {
        /// <summary>
        /// Fetches an entity using its unique identifier.
        /// </summary>
        /// <param name="id">Entity ID.</param>
        /// <returns>
        /// An <see cref="Operation{T}"/> containing the found entity or error details.
        /// </returns>
        Task<Operation<T>> ReadById(string id);
    }
}
