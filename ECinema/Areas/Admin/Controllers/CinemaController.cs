using ECinema.DataAccess;
using ECinema.Models;
using ECinema.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ECinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]

    public class CinemaController : Controller
    {
        //ApplicationDbContext _context = new();
        //Repository<Cinema> _CinemaRepository = new();
        private readonly IRepository<Cinema> _CinemaRepository; 
        public CinemaController(IRepository<Cinema> cinemaRepository)
        {
            _CinemaRepository = cinemaRepository;
        }
        public async Task<IActionResult> Index( CancellationToken cancellationToken)
        {
            var Cinemas = await _CinemaRepository.GetAllAsync(tracked:false);

            // Add Filter

            return View(Cinemas.Select(e => new
            {
                e.Id,
                e.Name,
                e.Description,
                e.Status,
            }).AsEnumerable());
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(Cinema cinema, IFormFile img, CancellationToken cancellationToken)
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
                cinema.Img = fileName;
            }

            // Save Cinema in db
            await _CinemaRepository.AddAsync(cinema, cancellationToken);
            await _CinemaRepository.CommitAsync(cancellationToken);

            //Response.Cookies.Append("success-notification", "Add Cinema Successfully");
            TempData["success-notification"] = "Add Cinema Successfully";

            //return View(nameof(Index));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id,CancellationToken cancellationToken)
        {
            var Cinema = await _CinemaRepository.GetOneAsync(e => e.Id == id, cancellationToken:cancellationToken);

            if (Cinema is null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(Cinema);
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(Cinema cinema, IFormFile? img,CancellationToken cancellationToken)
        {
            var CinemaInDb = await _CinemaRepository.GetOneAsync(e => e.Id == cinema.Id,tracked:false);
            if (CinemaInDb is null)
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
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", CinemaInDb.Img);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }

                    // Save Img in db
                    cinema.Img = fileName;
                }
            }
            else
            {
                cinema.Img = CinemaInDb.Img;
            }

            _CinemaRepository.Update(cinema);
            await _CinemaRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Update Cinema Successfully";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var cinema = await _CinemaRepository.GetOneAsync(e => e.Id == id);

            if (cinema is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", cinema.Img);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _CinemaRepository.Delet(cinema);
            await _CinemaRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Delete Cinema Successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}

