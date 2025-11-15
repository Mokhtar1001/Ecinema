using ECinema.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApplication2.DataAccess.EntityConfigration
{
    public class CartEntityTypeConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.HasKey(e => new { e.MovieId, e.ApplicationUserId });
        }
    }
}
