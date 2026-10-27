
using JobApplication.Application.Interfaces;

namespace JobApplication.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork,IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);

            if (!_repositories.TryGetValue(type, out var repository))
            {
                repository = new GenericRepository<TEntity>(_context);
                _repositories.Add(type, repository);
            }

            return (IGenericRepository<TEntity>)repository;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return _context.SaveChangesAsync(ct);
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
