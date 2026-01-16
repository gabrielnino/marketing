using Application.Result;
using Domain.WhatsApp.Redirect;

namespace Application.WhatsApp.UseCases.Repository.CRUD
{
    public interface ITrackedLinkCreate
    {
        Task<Operation<bool>> CreateAsync(TrackedLink entity);
    }
}
