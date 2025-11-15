using ECinema.Repositories.IRepositories;
using System.Threading.Tasks;

namespace ECinema.Repositories
{
    public class MovieRepository : Repository<Movie>, IMovieRepository
    {
        private ApplicationDbContext _context; //= new();

        public MovieRepository(ApplicationDbContext context): base(context)
        {
            _context = context;
        }

        public async Task AddRange(IEnumerable<Movie>movies,CancellationToken cancellationToken= default)
        {
            await _context.AddRangeAsync(movies,cancellationToken);
        }
    }
}
