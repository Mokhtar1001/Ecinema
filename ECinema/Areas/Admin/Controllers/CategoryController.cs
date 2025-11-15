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

    public class CategoryController : Controller
    {
        //ApplicationDbContext _context=new();
        private readonly IRepository<Category> _categoryRepository; /*= new Repository<Category>();*/

        public  CategoryController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var categorias = await _categoryRepository.GetAllAsync(tracked:false, cancellationToken:
                cancellationToken);
            return View(categorias.AsEnumerable());
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }
        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(Category category, CancellationToken cancellationToken)
        {
           await _categoryRepository.AddAsync(category, cancellationToken);
           await _categoryRepository.CommitAsync(cancellationToken);

            //return View(nameof(Index));
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id,CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id,cancellationToken:
                cancellationToken);

            if (category is null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(Category category, CancellationToken cancellationToken)
        {
            _categoryRepository.Update(category);
            await _categoryRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id,cancellationToken:
               cancellationToken );

            if (category is null)
                return RedirectToAction("NotFoundPage", "Home");

            _categoryRepository.Delet(category);
            await _categoryRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }
    }
}
