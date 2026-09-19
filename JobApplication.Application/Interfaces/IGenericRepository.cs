using System.Linq.Expressions;

namespace JobApplication.Infrastructure.Persistence
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
         Task AddAsync(TEntity entity, CancellationToken cancellationToken);

         Task EditAsync(TEntity enttiy, CancellationToken cancellationToken);

         Task RemoveAsync(TEntity enttiy, CancellationToken cancellationToken);


        Task<TEntity> GetByIdAsyncWithTracking(Expression<Func<TEntity, bool>> predicate,
                                            CancellationToken cancellationToken = default,
                                            params Expression<Func<TEntity, object?>>[]? includes
                                            );
        Task<TEntity> GetByIdAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate,
                                             CancellationToken cancellationToken = default,
                                             params Expression<Func<TEntity, object?>>[]? includes);

        Task<TEntity> GetByIdAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

         Task<IEnumerable<TResult>> GetAllAsync<TResult>(
                                             Expression<Func<TEntity, bool>>? predicate = null,
                                             Expression<Func<TEntity, TResult>>? selector = null,
                                             CancellationToken cancellationToken = default,
                                             params Expression<Func<TEntity, object?>>[]? includes) where TResult : class;

         Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate,
                                          CancellationToken cancellationToken = default);
    }
}
