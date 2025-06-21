using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Data;
using System.Linq;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách danh mục món ăn
        public IActionResult Index(string search)
        {
            var categories = _context.MenuCategories.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                categories = categories.Where(c => c.Name.Contains(search));

            ViewBag.Search = search;
            return View(categories.ToList());
        }

        // Hiển thị danh sách món ăn thuộc một danh mục
        public IActionResult Items(int id, string search)
        {
            var category = _context.MenuCategories.FirstOrDefault(c => c.MenuCategoryId == id);
            if (category == null) return NotFound();

            var items = _context.MenuItems.Where(m => m.MenuCategoryId == id);

            if (!string.IsNullOrEmpty(search))
                items = items.Where(m => m.Name.Contains(search));

            ViewBag.CategoryId = id;
            ViewBag.CategoryName = category.Name;
            ViewBag.Search = search;

            return View(items.ToList());
        }
    }
}
