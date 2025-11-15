using ECinema.DataAccess;
using ECinema.Models;
using ECinema.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ECinema.Repositories
{
    //Repository Pattern
    public class Repository<T> : IRepository<T> where T : class
    {
        private ApplicationDbContext _context; //= new();
        private DbSet<T> _db;
    public Repository(ApplicationDbContext context)
        {
            _context = context;
            _db = _context.Set<T>();
        }
        //CRUD Operations

        //C
        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _db.AddAsync(entity, cancellationToken);
            return entity;
        }

        //R Has 2 methods

        //1  Read All
        public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? expression = null, // filter condition
            Expression<Func<T, object>>[]? includes = null, //include navigation properties
            bool tracked = true,
            CancellationToken cancellationToken = default)
        {
            var entities = _db.AsQueryable();

            if (expression is not null)
                entities = entities.Where(expression);

            if(includes is not null)
            {
                foreach(var item in includes)
                {
                    entities = entities.Include(item);
                }
            }

            if (!tracked)
                entities = entities.AsNoTracking();

            return await entities.ToListAsync(cancellationToken);
        }
        //2 Read Single
        public async Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? expression = null, // filter condition
            Expression<Func<T, object>>[]? includes = null,//include navigation properties
            bool tracked = true,
            CancellationToken cancellationToken = default)
        {
            return (await GetAllAsync(expression,includes, tracked, cancellationToken)).FirstOrDefault();
        }

        //U
        public void Update(T entity)
        {
            _db.Update(entity);
        }

        //D
        public void Delet(T entity)
        {
            _db.Remove(entity);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
            await _context.SaveChangesAsync(cancellationToken);
            }
            catch(Exception ex)
            {
                Console.WriteLine($" Error: {ex.Message}");
            }

        }
    }
}
