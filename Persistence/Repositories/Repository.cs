using Microsoft.EntityFrameworkCore;
using Persistence.Context.Interface;

namespace Persistence.Repositories
{
    /// <summary>
    /// Base class for read operations on <typeparamref name="T"/> entities.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="context">EF Core context.</param>
    public abstract class Repository<T>(IUnitOfWork unitOfWork) where T : class
    {

        /// <summary>
        /// EF Core set for <typeparamref name="T"/>.
        /// </summary>
        protected readonly DbSet<T> _dbSet = unitOfWork.Context.Set<T>();
    }
}
