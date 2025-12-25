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
    public abstract class CreateRepository<T>(IUnitOfWork unitOfWork)
        : RepositoryCreate<T>(unitOfWork), ICreate<T> where T : class, IEntity
    {
        public async Task<Operation<bool>> CreateEntity(T entity)
        {
            await Create(entity);
            var success = CreateLabels.CreationSuccess;
            var message = string.Format(success, typeof(T).Name);
            return Operation<bool>.Success(true, message);
        }

        public async Task<Operation<bool>> CreateEntities(List<T> entities)
        {
            await CreateRange(entities);
            var success = CreateLabels.CreationSuccess;
            var message = string.Format(success, typeof(T).Name);
            return Operation<bool>.Success(true, message);
        }
    }
}
