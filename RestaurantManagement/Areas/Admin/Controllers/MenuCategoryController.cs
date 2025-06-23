using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using System.Linq;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MenuCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.MenuCategories
                .Include(c => c.MenuItems)
                .ToList();

            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(MenuCategory model)
        {
            if (ModelState.IsValid)
            {
                _context.MenuCategories.Add(model);
                _context.SaveChanges();
                TempData["Success"] = "Thêm danh mục thành công!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var category = _context.MenuCategories.Find(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(MenuCategory model)
        {
            if (ModelState.IsValid)
            {
                _context.MenuCategories.Update(model);
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật danh mục thành công!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var category = _context.MenuCategories.Find(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.MenuCategories.Find(id);
            if (category == null) return NotFound();

            // Kiểm tra xem danh mục còn món ăn nào không
            bool hasMenuItems = _context.MenuItems.Any(m => m.MenuCategoryId == id);
            if (hasMenuItems)
            {
                TempData["Error"] = "Không thể xoá danh mục vì vẫn còn món ăn trong danh mục này.";
                return RedirectToAction("Index");
            }

            _context.MenuCategories.Remove(category);
            _context.SaveChanges();

            TempData["Success"] = "Xoá danh mục thành công!";
            return RedirectToAction("Index");

        }

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
