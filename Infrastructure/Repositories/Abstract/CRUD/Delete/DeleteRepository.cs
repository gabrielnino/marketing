using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Persistence.Context.Interface;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Delete
{
    /// <summary>
    /// Base repository that implements deletion logic for entities of type T.
    /// </summary>
    public abstract class DeleteRepository<T>(IUnitOfWork unitOfWork)
        : RepositoryDelete<T>(unitOfWork), IDelete<T> where T : class, IEntity
    {
        /// <summary>
        /// Attempts to delete the entity with the given ID, returning a success result or a business failure if not found.
        /// </summary>
        public async Task<Operation<bool>> DeleteEntity(string id)
        {
            var entity = await HasId(id);
            if (entity is null)
            {
                var strategy = new BusinessStrategy<bool>();
                return OperationStrategy<bool>.Fail(DeleteLabels.EntityNotFound, strategy);
            }
            Delete(entity);
            var success = DeleteLabels.DeletionSuccess;
            var message = string.Format(success, typeof(T).Name);
            return Operation<bool>.Success(true, message);
        }
    }
}
