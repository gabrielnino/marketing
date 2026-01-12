using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Persistence.Context.Interface;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Create
{
    /// <summary>
    /// Base class to create entities with validation and error handling.
    /// </summary>
    public abstract class CreateRepository<T>(
        IUnitOfWork unitOfWork, 
        IErrorHandler errorHandler)
        : RepositoryCreate<T>(unitOfWork), ICreate<T> where T : class, IEntity
    {
        private readonly IErrorHandler ErrorHandler = errorHandler;


        public async Task<Operation<bool>> CreateEntity(T entity)
        {
            try
            {
                await Create(entity);
                var success = CreateLabels.CreationSuccess;
                var message = string.Format(success, typeof(T).Name);
                await unitOfWork.CommitAsync();
                return Operation<bool>.Success(true, message);
            }
            catch (Exception ex)
            {
                // Delegate to the error handler
                var fail = CreateLabels.CreationFail;
                var message = string.Format(fail, typeof(T).Name);
                return ErrorHandler.Fail<bool>(ex, message);
            }
        }

        public async Task<Operation<bool>> CreateEntities(List<T> entities)
        {
            await CreateRange(entities);
            var success = CreateLabels.CreationSuccess;
            var message = string.Format(success, typeof(T).Name);
            await unitOfWork.CommitAsync();
            return Operation<bool>.Success(true, message);
        }
    }
}
