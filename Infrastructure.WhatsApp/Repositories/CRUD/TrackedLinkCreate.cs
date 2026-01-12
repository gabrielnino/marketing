using Application.Result;
using Application.WhatsApp.UseCases.Repository.CRUD;
using Domain;
using Domain.WhatsApp.Redirect;
using Infrastructure.Repositories.Abstract.CRUD.Create;
using Persistence.Context.Interface;

namespace Infrastructure.WhatsApp.Repositories.CRUD
{
    public class TrackedLinkCreate(IUnitOfWork unitOfWork,
        IErrorHandler errorHandler) : CreateRepository<TrackedLink>(unitOfWork, errorHandler), ITrackedLinkCreate
    {
        public async Task<Operation<bool>> CreateAsync(TrackedLink entity)
        {
            return await CreateEntity(entity);
        }
    }
}
