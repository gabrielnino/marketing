using Application.Result;
using Application.WhatsApp.UseCases.Repository.CRUD;
using Domain.WhatsApp.Redirect;
using Infrastructure.Repositories.Abstract.CRUD.Create;
using Persistence.Context.Interface;

namespace Infrastructure.WhatsApp.Repositories.CRUD
{
    public class TrackedLinkUpdate(IUnitOfWork unitOfWork,
        IErrorHandler errorHandler) : CreateRepository<TrackedLink>(unitOfWork, errorHandler), ITrackedLinkUpdate
    {
        public async Task<Operation<bool>> UpdateAsync(TrackedLink entity)
        {
            return await UpdateAsync(entity);
        }
    }
}
