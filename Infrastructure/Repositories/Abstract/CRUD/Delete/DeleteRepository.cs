using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Infrastructure.Repositories.Abstract.CRUD.Create;
using Persistence.Context.Interface;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Delete
{
    /// <summary>
    /// Base repository that implements deletion logic for entities of type T.
    /// </summary>
    public abstract class DeleteRepository<T>(IUnitOfWork unitOfWork,
        IErrorHandler errorHandler)
        : RepositoryDelete<T>(unitOfWork), IDelete<T> where T : class, IEntity
    {
        private readonly IErrorHandler ErrorHandler = errorHandler;
        /// <summary>
        /// Attempts to delete the entity with the given ID, returning a success result or a business failure if not found.
        /// </summary>
        public async Task<Operation<bool>> DeleteEntity(string id)
        {
            try
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
                await unitOfWork.CommitAsync();
                return Operation<bool>.Success(true, message);
            }
            catch (Exception ex)
            {
                // Delegate to the error handler
                var fail = DeleteLabels.DeletionFailed;
                var message = string.Format(fail, typeof(T).Name);
                return ErrorHandler.Fail<bool>(ex, message);
            }
        }
    }
}
