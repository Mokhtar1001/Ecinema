using ECinema.DataAccess;
using ECinema.Repositories;
using ECinema.Repositories.IRepositories;

namespace ECinema
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services
            builder.Services.AddControllersWithViews();

            // DbContext
            builder.Services.AddDbContext<ApplicationDbContext>();

            // Generic repository
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // ✅ Specific repositories
            builder.Services.AddScoped<IMovieRepository, MovieRepository>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Admin}/{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
