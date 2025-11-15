using ECinema.DataAccess;
using ECinema.Models;
using ECinema.Utility;
using ECinema.ViewModels;
using ECinema.ViewModels.ECinema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ECinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]

    public class MovieController1 : Controller
    {
        private readonly ApplicationDbContext _context; //= new();
        private readonly IMovieRepository _movieRepository;  //= new();
        private readonly IRepository<Category> _CategoryRepository; //= new();
        private readonly IRepository<Cinema> _CinemaRepository; //= new();
        private readonly IRepository<MovieSubimage> _MovieSubimageRepository; // = new();
        public MovieController1(ApplicationDbContext context,
            IMovieRepository movieRepository,
            IRepository<Category> categoryRepository,
            IRepository<Cinema> cinemaRepository,
            IRepository<MovieSubimage> movieSubimageRepository)
        {
            _context = context;
            _movieRepository = movieRepository;
            _CategoryRepository = categoryRepository;
            _CinemaRepository = cinemaRepository;
            _MovieSubimageRepository = movieSubimageRepository;
        }
        public async Task<IActionResult> Index(FilterMovieVM filterMovieVM,CancellationToken cancellationToken, int page = 1)
        {

            var Movies = await _movieRepository.GetAllAsync(includes: [e=>e.Category,e=>e.Cinema],
                tracked: false,cancellationToken: cancellationToken);

            #region Filter Movie
            // Add Filter 
            if (filterMovieVM.Name is not null)
            {
                Movies = Movies.Where(e => e.Name.Contains(filterMovieVM.Name.Trim()));
                ViewBag.Name = filterMovieVM.Name;
            }

            if (filterMovieVM.Price is not null)
            {
                Movies = Movies.Where(e => e.Price==filterMovieVM.Price);
                ViewBag.minPrice = filterMovieVM.Price;
            }

            if (filterMovieVM.CategoryId is not null)
            {
                Movies = Movies.Where(e => e.CategoryId == filterMovieVM.CategoryId);
                ViewBag.CategoryId = filterMovieVM.CategoryId;
            }

            if (filterMovieVM.CinemaId is not null)
            {
                Movies = Movies.Where(e => e.CinemaId == filterMovieVM.CinemaId);
                ViewBag.CinemaId = filterMovieVM.CinemaId;
            }


            

            // Categories
            var categories = await _CategoryRepository.GetAllAsync(cancellationToken: cancellationToken);
            //ViewData["categories"] = categories.AsEnumerable();
            ViewBag.categories = categories.AsEnumerable();

            // Cinemas
            var Cinemas = await _CinemaRepository.GetAllAsync(cancellationToken: cancellationToken);
            ViewData["Cinemas"] = Cinemas.AsEnumerable();
            #endregion

            #region Pagination
            // Pagination
            ViewBag.TotalPages = Math.Ceiling(Movies.Count() / 8.0);
            ViewBag.CurrentPage = page;
            Movies = Movies.Skip((page - 1) * 8).Take(8); // 0 .. 8 
            #endregion

            return View(Movies.AsEnumerable());
        }
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            // Categories
            var categories = await _CategoryRepository.GetAllAsync(cancellationToken: cancellationToken);
            // Cinemas
            var Cinemas = await _CinemaRepository.GetAllAsync(cancellationToken: cancellationToken);

            return View(new MovieVM
            {
                Categories = categories.AsEnumerable(),
                Cinemas = Cinemas.AsEnumerable(),
            });
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(Movie Movie, IFormFile img, List<IFormFile>? subImgs,CancellationToken cancellationToken)
        {
            var transaction = _context.Database.BeginTransaction();

            try
            {
                if (img is not null && img.Length > 0)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        img.CopyTo(stream);
                    }

                    // Save Img in db
                    Movie.MainImg = fileName;
                }

                // Save Movie in db
                var MovieCreated = _movieRepository.AddAsync(Movie, cancellationToken: cancellationToken);
                await _movieRepository.CommitAsync(cancellationToken);

                if (subImgs is not null && subImgs.Count > 0)
                {
                    foreach (var item in subImgs)
                    {
                        // Save Img in wwwroot
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Movie_images", fileName);

                        using (var stream = System.IO.File.Create(filePath))
                        {
                            img.CopyTo(stream);
                        }

                        await _MovieSubimageRepository.AddAsync(new()
                        {
                            Img = fileName,
                            MovieId = MovieCreated.Id,
                        }, cancellationToken: cancellationToken);
                    }

                   await _MovieSubimageRepository.CommitAsync(cancellationToken);
                }


                TempData["success-notification"] = "Add movie Successfully";

                transaction.Commit();

            }
            catch (Exception ex)
            {
                // Logging
                TempData["error-notification"] = "Error While Saving the Movie";

                transaction.Rollback();

                // Validation
            }

            //return View(nameof(Index));
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]

        public async Task<IActionResult> Edit(int id,CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetOneAsync(e => e.Id == id, 
                includes: [e => e.MovieSubimages],cancellationToken: cancellationToken);

            if (movie is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Categories
            var categories = await _CategoryRepository.GetAllAsync(cancellationToken: cancellationToken);
            // Cinemas
            var cinemas = await _CinemaRepository.GetAllAsync(cancellationToken: cancellationToken);

            return View(new MovieVM
            {
                Categories = categories.AsEnumerable(),
                Cinemas = cinemas.AsEnumerable(),
                Movie = movie,
            });
        }



        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(Movie movie, IFormFile? img, List<IFormFile>? subImgs,CancellationToken cancellationToken)
        {
            var movieInDb = await _movieRepository.GetOneAsync(e => e.Id == movie.Id, tracked: false, cancellationToken: cancellationToken);
            if (movieInDb is null)
                return RedirectToAction("NotFoundPage", "Home");

            if (img is not null)
            {
                if (img.Length > 0)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        img.CopyTo(stream);
                    }

                    // Remove old Img in wwwroot
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", movieInDb.MainImg);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }

                    // Save Img in db
                    movie.MainImg = fileName;
                }
            }
            else
            {
                movie.MainImg = movieInDb.MainImg;
            }

            _movieRepository.Update(movie);
            await _movieRepository.CommitAsync(cancellationToken);

            if (subImgs is not null && subImgs.Count > 0)
            {
                movie.MovieSubimages = new List<MovieSubimage>();

                foreach (var item in subImgs)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\movie_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        item.CopyTo(stream);
                    }


                }

                await _MovieSubimageRepository.CommitAsync(cancellationToken);
            }




            TempData["success-notification"] = "Update movie Successfully";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetOneAsync(e=>e.Id == id,includes: [equals=>equals.MovieSubimages], cancellationToken: cancellationToken);

            if (movie is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", movie.MainImg);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            foreach (var item in movie.MovieSubimages)
            {
                var subImgOldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\movie_images", item.Img);
                if (System.IO.File.Exists(subImgOldPath))
                {
                    System.IO.File.Delete(subImgOldPath);
                }
            }


            _movieRepository.Delet(movie);
            await _movieRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Delete movie Successfully";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> DeleteSubImg(int movieId, string Img, CancellationToken cancellationToken )
        {
            var movieSubImgInDb = await _MovieSubimageRepository.GetOneAsync(e => e.MovieId == movieId && e.Img == Img);

            if (movieSubImgInDb is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\movie_images", movieSubImgInDb.Img);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _MovieSubimageRepository.Delet(movieSubImgInDb);
            await _movieRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Edit), new { id = movieId });
        }
    }
}
