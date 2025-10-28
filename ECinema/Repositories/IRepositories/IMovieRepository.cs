namespace ECinema.Repositories.IRepositories
{
    public class IMovieRepository: IRepository<Movie>
    {
        Task AddRange(IEnumerable<Movie> movies, CancellationToken cancellationToken = default);

    }

   
}
