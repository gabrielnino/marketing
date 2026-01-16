using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Result;
using Application.Result.Error;
using Application.WhatsApp.UseCases.Repository.CRUD;
using Domain.WhatsApp.Redirect;
using Microsoft.EntityFrameworkCore;
using Persistence.Context.Interface;
using Persistence.Repositories;

namespace Infrastructure.WhatsApp.Repositories.CRUD
{
    public class TrackedLinkRead(IUnitOfWork unitOfWork) :Read<TrackedLink>(unitOfWork), ITrackedLinkRead
    {
        public async Task<Operation<List<TrackedLink>>> ReadAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Operation<List<TrackedLink>>.Failure(
                    "Id cannot be null, empty, or whitespace.",
                    ErrorTypes.UserInput
                );

            var query = await ReadFilter(t => t.Id == id); // returns IQueryable<T> :contentReference[oaicite:1]{index=1}
            var items = await query.AsNoTracking().ToListAsync();

            return Operation<List<TrackedLink>>.Success(items);
        }
    }
}
