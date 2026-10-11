using JobApplication.Application.Patterns;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace JobApplication.Application.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
         Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

         Task EditAsync(TEntity enttiy, CancellationToken cancellationToken = default);

         Task RemoveAsync(TEntity enttiy, CancellationToken cancellationToken= default);


        Task<TEntity> GetByIdAsyncWithTracking(Expression<Func<TEntity, bool>> predicate,
                                            CancellationToken cancellationToken = default,
                                            params Expression<Func<TEntity, object?>>[]? includes
                                            );
        Task<TResult> GetByIdAsNoTrackingAsync<TResult>(Expression<Func<TEntity, bool>> predicate,
                                                      Expression<Func<TEntity, TResult>>? selector = default, 
                                                      CancellationToken cancellationToken = default,
                                                      params Expression<Func<TEntity, object?>>[]? includes);

        Task<TEntity> GetByIdAsync(Expression<Func<TEntity, bool>> predicate,
                                                CancellationToken cancellationToken = default,
                                                Expression<Func<TEntity, object?>>[]? includes  = null);

         Task<IEnumerable<TResult>> GetAllAsync<TResult>(
                                             Expression<Func<TEntity, bool>>? predicate = null,
                                             Expression<Func<TEntity, TResult>>? selector = null,
                                             CancellationToken cancellationToken = default,
                                             params Expression<Func<TEntity, object?>>[]? includes) where TResult : class;

        // Paging with search and sorting. selector projects entity to TResult.
        Task<(IEnumerable<TResult> Items, int TotalCount)> GetPagedAsync<TResult>(
                                            int page,
                                            int pageSize,
                                            Expression<Func<TEntity, bool>>? predicate = null,
                                            Expression<Func<TEntity, TResult>>? selector = null,
                                            string? orderBy = null,
                                            bool orderByDescending = false,
                                            string? searchTerm = null,
                                            CancellationToken cancellationToken = default,
                                            params Expression<Func<TEntity, object?>>[]? includes) where TResult : class;

         Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate,
                                          CancellationToken cancellationToken = default);

         Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null,
                              CancellationToken cancellationToken = default);

        Task<int> ExecuteUpdateAsync(
            Expression<Func<TEntity, bool>> predicate,
            Action<UpdateSettersBuilder<TEntity>> setters,
            CancellationToken cancellationToken = default);
    }
}
