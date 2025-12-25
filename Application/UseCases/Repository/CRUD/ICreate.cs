using Application.Result;
using Domain.Interfaces.Entity;

namespace Application.UseCases.Repository.CRUD
{
    /// <summary>
    /// Defines a contract for creating entities of type <typeparamref name="T"/>.
    /// </summary>
    public interface ICreate<T> where T : class, IEntity
    {
        // <summary>
        /// Attempts to create the specified <paramref name="entity"/> in the underlying data store.
        /// </summary>
        /// <param name="entity">The entity instance to persist.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed, yields an <see cref="Operation{Boolean}"/>
        /// indicating success or failure. The <see cref="Operation{Boolean}.Data"/> property will be
        /// <c>true</c> on success, and <see cref="Operation{Boolean}.Message"/> may contain
        /// additional context or error information.
        /// </returns>
        Task<Operation<bool>> CreateEntity(T entity);

        Task<Operation<bool>> CreateEntities(List<T> entity);
    }
}
