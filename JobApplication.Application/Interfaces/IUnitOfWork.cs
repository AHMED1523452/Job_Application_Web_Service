using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Infrastructure.Persistence
{
    public interface IUnitOfWork
    {
        //. implementation of the repository pattern
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class; 
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
