using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Result;
using Domain.WhatsApp.Redirect;

namespace Application.WhatsApp.UseCases.Repository.CRUD
{
    public interface ITrackedLinkRead
    {
        Task<Operation<List<TrackedLink>>> ReadAsync(string id);
    }
}
