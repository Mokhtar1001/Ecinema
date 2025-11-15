using ECinema.Configrations;
using ECinema.Utility.DBInitilizer;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;

namespace ECinema
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var connectionStrng =
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string " +
                "'DefaultConnection' not found.");

           //AppConfigrution.RegisterConfig(builder.Services, connectionStrng);

            builder.Services.RegisterConfig(connectionStrng);
            builder.Services.RegisterMapsterConfig();



            var app = builder.Build();
            // Initialize Database
            var scope = app.Services.CreateScope();
            var service = scope.ServiceProvider.GetService<IDBInitializer>();
            service!.Initialize();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            

            app.Run();
        }
    }
}
