using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Infrastructure.Repositories.Abstract.CRUD.Create;
using Persistence.Context.Interface;

namespace Infrastructure.Repositories.CRUD
{
    using ErrorLog = Domain.ErrorLog;
    public class ErrorLogCreate(IUnitOfWork unitOfWork) : CreateRepository<ErrorLog>(unitOfWork), IErrorLogCreate
    {
        public async Task<Operation<bool>> CreateAsync(ErrorLog entity)
        {
            return await CreateEntity(entity);
        }
    }
}
