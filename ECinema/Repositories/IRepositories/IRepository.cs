using System.Linq.Expressions;

namespace ECinema.Repositories.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);


        //R Has 2 methods

        //1  Read All
         Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? expression = null, // filter condition
            Expression<Func<T, object>>[]? includes = null, //include navigation properties
            bool tracked = true,
            CancellationToken cancellationToken = default);

        //2 Read Single
         Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? expression = null, // filter condition
            Expression<Func<T, object>>[]? includes = null,//include navigation properties
            bool tracked = true,
            CancellationToken cancellationToken = default);


        //U
         void Update(T entity);


        //D
         void Delet(T entity);


         Task CommitAsync(CancellationToken cancellationToken = default);
        
    }
}
