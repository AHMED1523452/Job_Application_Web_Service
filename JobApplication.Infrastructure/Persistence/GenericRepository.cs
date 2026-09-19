using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace JobApplication.Infrastructure.Persistence
{
    public class GenericRepository<TEntity>  : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly AppDbContext dbContext;

        public GenericRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken) => await dbContext.Set<TEntity>().AddAsync(entity);

        public async Task EditAsync(TEntity enttiy, CancellationToken cancellationToken) =>  dbContext.Set<TEntity>().Update(enttiy);

        public async Task RemoveAsync(TEntity enttiy, CancellationToken cancellationToken) => dbContext.Set<TEntity>().Remove(enttiy);


        //. Focus on it (With bringing the model or DTO with tracking)
        //. params word here is used to specify a variable number of arguments for the includes parameter.
        //. It allows you to pass in multiple expressions that define the related entities to include in the query. The includes parameter is an array of expressions, and you can pass in as many expressions as needed when calling the method.
        public async Task<TEntity> GetByIdAsyncWithTracking(Expression<Func<TEntity, bool>> predicate, 
                                                         CancellationToken cancellationToken = default,
                                                         params Expression<Func<TEntity, object?>>[]? includes
                                                         )
        {
            IQueryable<TEntity> query = dbContext.Set<TEntity>(); 
            if(includes != null || includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return await query.Where(predicate).FirstOrDefaultAsync();
        }

        //. Focus on it (With bringing the model or DTO without tracking)
        public async Task<TEntity> GetByIdAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, 
                                                         CancellationToken cancellationToken = default,
                                                         params Expression<Func<TEntity, object?>>[]? includes )
        {
            IQueryable<TEntity> query = dbContext.Set<TEntity>(); 
            if(includes != null || includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            query = query.AsNoTracking().Where(predicate);



            return await query.FirstOrDefaultAsync();
        }

        public async Task<TEntity> GetByIdAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await dbContext.Set<TEntity>().AsNoTracking().Where(predicate).FirstOrDefaultAsync();

        //. Without tracking, and with bringing the model or DTO
        public async Task<IEnumerable<TResult>> GetAllAsync<TResult>(
                                                         Expression<Func<TEntity, bool>>? predicate = null,
                                                         Expression<Func<TEntity, TResult>>? selector = null,
                                                         CancellationToken cancellationToken = default,
                                                         params Expression<Func<TEntity, object?>>[]? includes) where TResult : class
        {
            IQueryable<TEntity> query = dbContext.Set<TEntity>();
            if (includes != null || includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            
            if(predicate !=  null) 
                return await query.AsNoTracking().Where(predicate).Select(selector).ToListAsync();

            return await query.AsNoTracking().Select(selector).ToListAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate ,
                                          CancellationToken cancellationToken = default)
        {
            return await dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
        }
    }
}
