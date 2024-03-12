using UserMicroservices.Models.Domain.Entities;

namespace UserMicroservices.Repository.IRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<bool> CreateAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync();
    }
}
