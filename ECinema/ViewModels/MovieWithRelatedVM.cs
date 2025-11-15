namespace ECinema.ViewModels
{
    public class MovieWithRelatedVM
    {
        public Movie  Movie { get; set; } = default!;

        public List<Movie> RelatedMovies { get; set; } = [];
    }
}
