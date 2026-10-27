using JobApplication.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace JobApplication.Infrastructure.Persistence
{
    public class GenericRepository<TEntity>  : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly AppDbContext dbContext;

        public GenericRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => await dbContext.Set<TEntity>().AddAsync(entity);

        public async Task EditAsync(TEntity enttiy, CancellationToken cancellationToken = default) =>  dbContext.Set<TEntity>().Update(enttiy);

        public async Task RemoveAsync(TEntity enttiy, CancellationToken cancellationToken = default) => dbContext.Set<TEntity>().Remove(enttiy);


        //. Focus on it (With bringing the model or DTO with tracking)
        //. params word here is used to specify a variable number of arguments for the includes parameter.
        //. It allows you to pass in multiple expressions that define the related entities to include in the query. The includes parameter is an array of expressions, and you can pass in as many expressions as needed when calling the method.
        public async Task<TEntity> GetByIdAsyncWithTracking(Expression<Func<TEntity, bool>> predicate, 
                                                         CancellationToken cancellationToken = default,
                                                         params Expression<Func<TEntity, object?>>[]? includes
                                                         )
        {
            IQueryable<TEntity> query = dbContext.Set<TEntity>(); 
            if(includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return await query.Where(predicate).FirstOrDefaultAsync();
        }


        public async Task<TResult> GetByIdAsyncWithTracking<TResult>(Expression<Func<TEntity, bool>> predicate, 
                                                            Expression<Func<TEntity, TResult>> selector,
                                                            CancellationToken cancellationToken = default,
                                                            params Expression<Func<TEntity, object?>>[]? includes) 
        {
            IQueryable<TEntity> query = dbContext.Set<TEntity>(); 
            if(includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return await query.Where(predicate).Select(selector).FirstOrDefaultAsync();
        }

        //. Focus on it (With bringing the model or DTO without tracking)
        public async Task<TResult> GetByIdAsNoTrackingAsync<TResult>(Expression<Func<TEntity, bool>> predicate, 
                                                            Expression<Func<TEntity, TResult>>? selector = default ,
                                                            CancellationToken cancellationToken = default,
                                                            params Expression<Func<TEntity, object?>>[]? includes )
        {
            IQueryable<TEntity> query = dbContext.Set<TEntity>(); 
            if(includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            query = query.AsNoTracking().Where(predicate);

            if (selector == null)
            {
                // if TResult is same as TEntity return the entities directly, otherwise require selector
                if (typeof(TResult) == typeof(TEntity))
                {
                    if (predicate != null)
                    {
                        var obj = await query.AsNoTracking().Where(predicate).FirstOrDefaultAsync();
                        return (TResult)(object)obj;
                    }
                    var all = await query.AsNoTracking().FirstOrDefaultAsync();
                    return (TResult)(object)all;
                }

                throw new ArgumentNullException(nameof(selector), "Selector must be provided for projection when TResult differs from TEntity.");
            }
            return await query.Select(selector).FirstOrDefaultAsync();
        }

        //. Without no tracking or includes just predicate 
        public async Task<TEntity> GetByIdAsync(Expression<Func<TEntity, bool>> predicate,
                                                CancellationToken cancellationToken = default,
                                                Expression<Func<TEntity, object?>>[]? includes = null)
        {
            IQueryable<TEntity> query = dbContext.Set<TEntity>();
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            query = query.AsNoTracking().Where(predicate);

            return await query.FirstOrDefaultAsync();
        }

        //. Without tracking, and with bringing the model or DTO
        public async Task<IEnumerable<TResult>> GetAllAsync<TResult>(
                                                         Expression<Func<TEntity, bool>>? predicate = null,
                                                         Expression<Func<TEntity, TResult>>? selector = null,
                                                         CancellationToken cancellationToken = default,
                                                         params Expression<Func<TEntity, object?>>[]? includes)
        {
            IQueryable<TEntity> query = dbContext.Set<TEntity>();
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    //. spliting or separating each query in SQL
                    //.not meaning that imporve the execution time but for don't make many joins operations
                    query = query.Include(include).AsSplitQuery();
                }
            }
            
            if (selector == null)
            {
                // if TResult is same as TEntity return the entities directly, otherwise require selector
                if (typeof(TResult) == typeof(TEntity))
                {
                    if (predicate != null)
                    {
                        var list = await query.AsNoTracking().Where(predicate).ToListAsync();
                        return (IEnumerable<TResult>)(object)list;
                    }
                    var all = await query.AsNoTracking().ToListAsync();
                    return (IEnumerable<TResult>)(object)all;
                }

                throw new ArgumentNullException(nameof(selector),
                            "Selector must be provided for projection when TResult differs from TEntity.");
            }

            if (predicate != null)
                return await query.AsNoTracking().Where(predicate).Select(selector).ToListAsync();

            return await query.AsNoTracking().Select(selector).ToListAsync();
        }

        // Paging with search and sorting
        public async Task<(IEnumerable<TResult> Items, int TotalCount)> GetPagedAsync<TResult>(
                                            int page,
                                            int pageSize,
                                            Expression<Func<TEntity, bool>>? predicate = null,
                                            Expression<Func<TEntity, TResult>>? selector = null,
                                            string? orderBy = null,
                                            bool orderByDescending = false,
                                            string? searchTerm = null,
                                            CancellationToken cancellationToken = default,
                                            params Expression<Func<TEntity, object?>>[]? includes) where TResult : class
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector), "Selector must be provided for paged queries.");

            IQueryable<TEntity> query = dbContext.Set<TEntity>();
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include).AsSplitQuery();
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Basic search: try to apply search against any string property -- best-effort
                var parameter = Expression.Parameter(typeof(TEntity), "x");
                Expression? searchExpr = null;
                foreach (var prop in typeof(TEntity).GetProperties())
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        MemberExpression member = Expression.Property(parameter, prop);
                        //. The Array must be existed here  (don't  forget) 
                        //. Try to create function like the Contains function that existing in string functions 
                       //. and we will search about the SearchTerm that existing as parameter
                        MethodCallExpression contains =
                         Expression.Call(member, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, Expression.Constant(searchTerm));
                        searchExpr = searchExpr == null ? contains : Expression.OrElse(searchExpr, contains);
                    }
                }

                if (searchExpr != null)
                {
                    var lambda = Expression.Lambda<Func<TEntity, bool>>(searchExpr, parameter);
                    query = query.Where(lambda);
                }
            }

            if (predicate != null)
                query = query.Where(predicate);

            var totalCount = await query.CountAsync(cancellationToken);

            // Apply ordering if provided
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                var prop = typeof(TEntity).GetProperty(orderBy);
                if (prop != null)
                {
                    var param = Expression.Parameter(typeof(TEntity), "x");
                    var body = Expression.Property(param, prop);
                    var lambda = Expression.Lambda(body, param);
                    var methodName = orderByDescending ? "OrderByDescending" : "OrderBy";
                    var resultExpr = Expression.Call(typeof(Queryable), methodName,
                                                     new Type[] { typeof(TEntity), prop.PropertyType }, 
                                                     query.Expression, Expression.Quote(lambda));
                    query = query.Provider.CreateQuery<TEntity>(resultExpr);
                }
            }

            //. if greater than 0 take it and deal with 
            if (pageSize > 0)
            {
                query = query.Skip((page - 1) * pageSize).Take(pageSize);
            }

            //. if not ignore it 
            var projected = query.AsNoTracking().Select(selector);
            var items = await projected.ToListAsync(cancellationToken);
            return (items, totalCount);
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate ,
                                          CancellationToken cancellationToken = default)
            => await dbContext.Set<TEntity>().AsNoTracking().AsQueryable().AnyAsync(predicate, cancellationToken);

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null,
                                          CancellationToken cancellationToken = default)
        {
            var query = dbContext.Set<TEntity>().AsNoTracking();
            return predicate is null
                ? await query.CountAsync(cancellationToken)
                : await query.CountAsync(predicate, cancellationToken);
        }

   
        //. don't forget the built in class UpdateSetterBuilder
        public Task<int> ExecuteUpdateAsync(
            Expression<Func<TEntity, bool>> predicate,
            Action<UpdateSettersBuilder<TEntity>> setters, //. taking a lambda expression of type TEntity that should be class
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(setters);
            return dbContext.Set<TEntity>()
                .Where(predicate)
                .ExecuteUpdateAsync(setters, cancellationToken);
        }
    }
}
