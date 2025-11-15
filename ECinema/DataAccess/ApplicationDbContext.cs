using Microsoft.EntityFrameworkCore;
using WebApplication2.DataAccess.EntityConfigration;
using ECinema.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ECinema.ViewModels;
using Umbraco.Core.Models.Membership;



namespace ECinema.DataAccess

{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options)
            : base(options)
        {
        }
        public DbSet<Cinema> Cinemas{ get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<MovieSubimage> MovieSubimages{ get; set; }
        public DbSet<MovieActor>MovieActors{ get; set; }
        public DbSet<ApplicationUserOTP> ApplicationUserOTPs{ get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Seat> Seats { get; set; }
    


        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);

        //    optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=ECinema;Integrated Security=True;Connect Timeout=30;" +
        //        "Encrypt=True;Trust Server Certificate=True;" +
        //        "Application Intent=ReadWrite;Multi Subnet Failover=False");
        //}


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MovieImageEntityTypeconfigrations).Assembly);
            // إضافة 50 كرسي
            for (int i = 1; i <= 50; i++)
            {
                modelBuilder.Entity<Seat>().HasData(new Seat
                {
                    Id = i,
                    SeatCode = "S" + i,
                    IsReserved = false
                });
            }

            base.OnModelCreating(modelBuilder);

        }
        
        
    }
}
