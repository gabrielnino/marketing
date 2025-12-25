using Application.Result;
using Domain;

namespace Application.UseCases.Repository.UseCases.CRUD
{
    public interface IErrorLogCreate
    {
        Task<Operation<bool>> CreateInvoiceAsync(ErrorLog entity);
    }
}
