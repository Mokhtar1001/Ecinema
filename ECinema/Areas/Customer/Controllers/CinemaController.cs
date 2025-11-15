using ECinema.DataAccess; // عدل حسب مكان DbContext
using ECinema.Models; // عدل حسب مكان Model
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace YourProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CinemaController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CinemaController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var reserved = _db.Seats
                              .Where(x => x.IsReserved)
                              .Select(x => x.SeatCode)
                              .ToList();

            ViewBag.Reserved = reserved;
            return View();
        }

        [HttpPost]
        public IActionResult BookSeats(string seats)
        {
            if (string.IsNullOrEmpty(seats))
            {
                TempData["Error"] = "لم يتم اختيار أي كرسي للحجز!";
                return RedirectToAction("Index");
            }

            var seatList = seats.Split(',').ToList();
            var alreadyReserved = new List<string>();

            foreach (var seatCode in seatList)
            {
                var seat = _db.Seats.FirstOrDefault(x => x.SeatCode == seatCode);
                if (seat != null)
                {
                    if (seat.IsReserved)
                        alreadyReserved.Add(seatCode);
                    else
                        seat.IsReserved = true;
                }
            }

            _db.SaveChanges();

            if (alreadyReserved.Count > 0)
                TempData["Error"] = "الكراسي التالية محجوزة بالفعل: " + string.Join(", ", alreadyReserved);
            else
                TempData["Success"] = "تم حجز المقاعد بنجاح!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        
        public IActionResult CancelSeats(string seats)
        {
            if (string.IsNullOrEmpty(seats))
            {
                TempData["Error"] = "لم يتم اختيار أي كرسي لإلغاء الحجز!";
                return RedirectToAction("Index");
            }

            var seatList = seats.Split(',').ToList();

            foreach (var seatCode in seatList)
            {
                var seat = _db.Seats.FirstOrDefault(x => x.SeatCode == seatCode);
                if (seat != null && seat.IsReserved)
                {
                    seat.IsReserved = false;
                }
            }

            _db.SaveChanges();
            TempData["Success"] = "تم إلغاء حجز المقاعد بنجاح!";

            return RedirectToAction("Index");
        }
    }
}