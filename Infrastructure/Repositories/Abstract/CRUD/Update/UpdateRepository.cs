using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Infrastructure.Repositories.Abstract.CRUD.Delete;
using Infrastructure.Result;
using Persistence.Context.Interface;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Update
{
    // Repository for updating entities: verifies existence, applies changes, and returns an Operation result.
    public abstract class UpdateRepository<T>(
        IUnitOfWork unitOfWork,
        IErrorHandler errorHandler)
        : RepositoryUpdate<T>(unitOfWork), IUpdate<T>
        where T : class, IEntity
    {
        private readonly IErrorHandler ErrorHandler = errorHandler;
        // Attempts to update the given entity or returns a BusinessValidation failure if not found.
        public async Task<Operation<bool>> UpdateEntity(T modify)
        {
            try
            {
                var entity = await HasId(modify.Id);
                if (entity is null)
                {
                    var strategy = new BusinessStrategy<bool>();
                    return OperationStrategy<bool>.Fail(UpdateLabels.EntityNotFound, strategy);
                }

                var modified = ApplyUpdates(modify, entity);
                Update(modified);

                var success = UpdateLabels.UpdationSuccess;
                var message = string.Format(success, typeof(T).Name);
                await unitOfWork.CommitAsync();
                return Operation<bool>.Success(true, message);
            }
            catch (Exception ex)
            {
                // Delegate to the error handler
                var fail = UpdateLabels.UpdationFail;
                var message = string.Format(fail, typeof(T).Name);
                return ErrorHandler.Fail<bool>(ex, message);
            }
        }

        // Defines how to copy properties from the modified instance onto the existing entity.
        public abstract T ApplyUpdates(T modified, T unmodified);
    }
}
