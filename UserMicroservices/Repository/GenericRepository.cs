﻿using Microsoft.EntityFrameworkCore;
using UserMicroservices.Data;
using UserMicroservices.Repository.IRepository;

namespace UserMicroservices.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly UserDbContext _dbContext;
        public DbSet<T> table;
        public GenericRepository(UserDbContext dbContext)
        {
            _dbContext = dbContext;
            table = _dbContext.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await Task.FromResult(table.ToList());
        public async Task<T> GetByIdAsync(int id) => await table.FindAsync(id);
        public async Task<T> GetByEmailAsync(string email) => await table.FindAsync(email);
        public async Task<bool> CreateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            table.Add(entity);
            return await SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            table.Update(entity);
            return await SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            table.Remove(entity);
            return await SaveChangesAsync();

        }

        public async virtual Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
