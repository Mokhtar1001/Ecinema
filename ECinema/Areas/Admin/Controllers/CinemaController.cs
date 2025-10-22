using ECinema.DataAccess;
using ECinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECinema.Areas.Admin.Controllers
{
    public class CinemaController : Controller
    {
        ApplicationDbContext _context =new();
        public IActionResult Index()
        {
            var cinemas = _context.Cinemas.AsNoTracking().AsQueryable();

            // Add Filter

            return View(cinemas.Select(e => new
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
        public IActionResult Create(Cinema cinema, IFormFile img)
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

            // Save brand in db
            _context.Cinemas.Add(cinema);
            _context.SaveChanges();

            //Response.Cookies.Append("success-notification", "Add Brand Successfully");
            TempData["success-notification"] = "Add Brand Successfully";

            //return View(nameof(Index));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var brand = _context.Cinemas.FirstOrDefault(e => e.Id == id);

            if (brand is null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(brand);
        }

        [HttpPost]
        public IActionResult Edit(Cinema cinema, IFormFile? img)
        {
            var cinemaInDb = _context.Cinemas.AsNoTracking().FirstOrDefault(e => e.Id == cinema.Id);
            if (cinemaInDb is null)
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
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", cinemaInDb.Img);
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
                cinema.Img = cinemaInDb.Img;
            }

            _context.Cinemas.Update(cinema);
            _context.SaveChanges();

            TempData["success-notification"] = "Update Brand Successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var cinema = _context.Cinemas.FirstOrDefault(e => e.Id == id);

            if (cinema is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", cinema.Img);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _context.Cinemas.Remove(cinema);
            _context.SaveChanges();

            TempData["success-notification"] = "Delete Brand Successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}

