using Application.Result;
using Domain.WhatsApp.Redirect;

namespace Application.WhatsApp.UseCases.Repository.CRUD
{
    public interface ITrackedLinkUpdate
    {
        Task<Operation<bool>> UpdateAsync(TrackedLink entity);
    }
}
