using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace AMS.Entities.Infrastructure.Repository.IRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<bool> CreateAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(T entity);
        Task<T> GetByEmailAsync(string email);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> includes = null);
    }
}
