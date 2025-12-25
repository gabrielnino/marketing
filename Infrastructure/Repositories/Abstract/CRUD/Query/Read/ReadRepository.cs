using Domain.Interfaces.Entity;
using Application.Result;
using Microsoft.EntityFrameworkCore;
using Application.Common.Pagination;
using System.Linq.Expressions;
using Persistence.Context.Interface;

namespace Infrastructure.Repositories.Abstract.CRUD.Query.Read
{
    /// <summary>
    /// Provides cursor‐based pagination for querying entities of type T.
    /// </summary>
    public abstract class ReadRepository<T>(
        IUnitOfWork unitOfWork,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy
    ) where T : class, IEntity
    {
        /// <summary>
        /// Executes a filtered, ordered, and cursor‐based paged query.
        /// </summary>
        public async Task<Operation<PagedResult<T>>> GetPageAsync(
            Expression<Func<T, bool>>? filter,
            string? cursor,
            int pageSize
        )
        {
            var query = BuildBaseQuery(filter);
            var count = query.Count();

            if (!string.IsNullOrEmpty(cursor))
                query = ApplyCursorFilter(query, cursor);

            var items = await query.Take(pageSize + 1).ToListAsync();
            var next = BuildNextCursor(items, pageSize);

            if (next != null)
                items.RemoveAt(pageSize);

            var result = new PagedResult<T>
            {
                Items = items,
                NextCursor = next,
                TotalCount = count
            };

            return Operation<PagedResult<T>>.Success(result);
        }

        public async Task<Operation<PagedResult<T>>> GetAllMembers(CancellationToken cancellationToken = default)
        {
            var query = unitOfWork.Context.Set<T>().AsNoTracking();
            var items = await query.ToListAsync(cancellationToken);
            var result = new PagedResult<T>
            {
                Items = items,
                NextCursor = null,
                TotalCount = items.Count
            };

            return Operation<PagedResult<T>>.Success(result);
        }

        /// <summary>
        /// Builds the base query, applying the optional filter and ordering.
        /// </summary>
        protected virtual IQueryable<T> BuildBaseQuery(Expression<Func<T, bool>>? filter)
        {
            var q = unitOfWork.Context.Set<T>().AsNoTracking();
            if (filter != null)
            {
                q = q.Where(filter);
            }
            return orderBy(q);
        }

        /// <summary>
        /// Applies the cursor filter to the query to continue from the previous page.
        /// </summary>
        protected abstract IQueryable<T> ApplyCursorFilter(IQueryable<T> query, string cursor);

        /// <summary>
        /// Constructs the next cursor from the fetched items, or returns null if done.
        /// </summary>
        protected abstract string? BuildNextCursor(List<T> items, int size);
    }
}
