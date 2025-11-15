using ECinema.Utility;
using ECinema.Utility.DBInitilizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace ECinema
{
    public static class AppConfigrution
    {
        public static void RegisterConfig(this IServiceCollection services, string connection)
        {
            services.AddDbContext<ApplicationDbContext>(Option =>
            {
                // Option.UseSqlServer(builder.Configuration["ConnectionStrings : DefaultConnection"]);
                Option.UseSqlServer(connection);
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(option =>
            {
                option.Password.RequiredLength = 8;
                option.Password.RequireNonAlphanumeric = false;
                option.User.RequireUniqueEmail = true;
                option.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

                services.ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = "/Identity/Account/Login"; // Default login path
                    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Default access denied path
                });


            services.AddTransient<IEmailSender, EmailSender>();

            services.AddScoped<IRepository<Category>, Repository<Category>>();
            services.AddScoped<IRepository<Cinema>, Repository<Cinema>>();
            services.AddScoped<IRepository<Movie>, Repository<Movie>>();
            services.AddScoped<IMovieRepository, MovieRepository>();
            services.AddScoped<IRepository<MovieSubimage>, Repository<MovieSubimage>>();
            services.AddScoped<IRepository<ApplicationUserOTP>, Repository<ApplicationUserOTP>>();
            services.AddScoped<IRepository<Cart>, Repository<Cart>>();
            services.AddScoped<IRepository<Promotion>, Repository<Promotion>>();


            services.AddScoped<IDBInitializer, DBInitializer>();
        }
    }
}
