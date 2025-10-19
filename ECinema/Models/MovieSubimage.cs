using Microsoft.EntityFrameworkCore;

namespace ECinema.Models
{
    [PrimaryKey(nameof(MovieId),nameof(Id))]

    public class MovieSubimage
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; } = null!;
        public string Img { get; set; } = string.Empty;
    }
}
