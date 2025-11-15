using ECinema.ViewModels.ECinema.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;

namespace ECinema.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        public IActionResult Index(FilterMovieVM filterMovieVM, int page = 1)
        {
           
            var movies = _context.Movies/*.AsNoTracking()*/.Include(e => e.Category).AsQueryable();

            // Add Filter 
            if (filterMovieVM.Name is not null)
            {
                movies = movies.Where(e => e.Name.Contains(filterMovieVM.Name));
                ViewBag.Name = filterMovieVM.Name;
            }

            if (filterMovieVM.Price is not null)
            {
                movies = movies.Where(e => e.Price == filterMovieVM.Price);

            }


            if (filterMovieVM.CategoryId is not null)
            {
                movies = movies.Where(e => e.CategoryId == filterMovieVM.CategoryId);
                ViewBag.CategoryId = filterMovieVM.CategoryId;
            }

            if (filterMovieVM.CinemaId is not null)
            {
                movies = movies.Where(e => e.CinemaId == filterMovieVM.CinemaId);
                ViewBag.CinemaId = filterMovieVM.CinemaId;
            }

            // Categories
            var categories = _context.Categories;
            //ViewData["categories"] = categories.AsEnumerable();
            ViewBag.categories = categories.AsEnumerable();

            // Cinema
            var cinemas = _context.Cinemas;
            ViewData["cinemas"] = cinemas.AsEnumerable();

            // Pagination
            ViewBag.TotalPages = Math.Ceiling(movies.Count() / 8.0);
            ViewBag.CurrentPage = page;
            movies = movies.Skip((page - 1) * 8).Take(8); // 0 .. 8

            return View(movies.AsEnumerable()); ;
        }

        private readonly ILogger<HomeController> _logger;
        private ApplicationDbContext _context;// = new();

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }




        public async Task<IActionResult> Item(int id, CancellationToken cancellationToken)
        {
            var movie = await _context.Movies.Include(e => e.Category).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (movie is null)
                return NotFound();

            movie.Traffic += 1;
            _context.SaveChanges();

            var relatedMovies = await _context.Movies.Where(e => e.Name.Contains(movie.Name) && e.Id != movie.Id).OrderBy(e => e.Traffic).Skip(0).Take(4).ToListAsync();

            return View(new MovieWithRelatedVM
            {
                Movie = movie,
                RelatedMovies = relatedMovies
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public ViewResult Welcome()
        {
            return View();
        }

        public ViewResult PersonalInfo(int id)
        {
            List<Actor> actors = new List<Actor>();
            //persons.AddRange(new()
            //{
            //    Id = 1,
            //    Name = "Mohamed",
            //    Salary = 1000,
            //    Address = "Caio"
            //}, new()
            //{
            //    Id = 2,
            //    Name = "Ali",
            //    Salary = 2000,
            //    Address = "Alex"
            //});

            // Logic

            var personsInDB = actors.AsQueryable();
            var totalActors = actors.Count();

            // Add Filter
            personsInDB = personsInDB.Where(e => e.Id == id);

            return View(new ActorVM
            {
                Actors = personsInDB.ToList(),
                TotalActors = totalActors
            });
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

