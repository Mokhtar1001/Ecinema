namespace ECinema.Repositories.IRepositories
{
    public interface IMovieRepository : IRepository<Movie>
    {
        Task AddRange(IEnumerable<Movie> movies, CancellationToken cancellationToken = default);
        
    }
}
