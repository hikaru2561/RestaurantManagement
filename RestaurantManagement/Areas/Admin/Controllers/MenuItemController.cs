using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using System;
using System.IO;
using System.Linq;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MenuItemController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Create(int id) // id là MenuCategoryId
        {
            var category = _context.MenuCategories.Find(id);
            if (category == null) return NotFound();

            ViewBag.CategoryName = category.Name;
            ViewBag.MenuCategoryId = id;
            ViewBag.Ingredients = _context.InventoryItems.ToList();

            return View(new MenuItem { MenuCategoryId = id });
        }
        [HttpPost]
        public IActionResult Create(MenuItem model, IFormFile imageFile, int[] IngredientIds, float[] Quantities)
        {
            if (ModelState.IsValid)
            {
                model.Status = true;
                model.ImagePath = "";

                _context.MenuItems.Add(model);
                _context.SaveChanges(); // Có MenuItemId

                // Xử lý ảnh
                if (imageFile != null && imageFile.Length > 0)
                {
                    var ext = Path.GetExtension(imageFile.FileName);
                    var fileName = $"{model.MenuItemId}{ext}";
                    var folder = Path.Combine(_env.WebRootPath, "images", "MenuItem");
                    Directory.CreateDirectory(folder);
                    var path = Path.Combine(folder, fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }
                    model.ImagePath = fileName;
                    _context.SaveChanges();
                }

                // Lưu nguyên liệu sử dụng
                for (int i = 0; i < IngredientIds.Length; i++)
                {
                    if (Quantities[i] > 0)
                    {
                        _context.InventoryUsages.Add(new InventoryUsage
                        {
                            MenuItemId = model.MenuItemId,
                            InventoryItemId = IngredientIds[i],
                            QuantityUsed = (int)Quantities[i]
                        });
                    }
                }
                _context.SaveChanges();

                TempData["Success"] = "Thêm món ăn thành công!";
                return RedirectToAction("Items", "Menu", new { id = model.MenuCategoryId });
            }

            var category = _context.MenuCategories.Find(model.MenuCategoryId);
            ViewBag.CategoryName = category?.Name;
            ViewBag.MenuCategoryId = model.MenuCategoryId;
            ViewBag.Ingredients = _context.InventoryItems.ToList();
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var item = _context.MenuItems.Find(id);
            if (item == null) return NotFound();

            ViewBag.MenuCategoryId = item.MenuCategoryId;
            ViewBag.CategoryName = _context.MenuCategories
                .FirstOrDefault(c => c.MenuCategoryId == item.MenuCategoryId)?.Name;

            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(MenuItem model, IFormFile? imageFile)
        {
            var item = _context.MenuItems.Find(model.MenuItemId);
            if (item == null) return NotFound();

            if (ModelState.IsValid)
            {
                item.Name = model.Name;
                item.Price = model.Price;
                item.Status = model.Status;

                if (imageFile != null && imageFile.Length > 0)
                {        
                    if (!string.IsNullOrEmpty(item.ImagePath))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, "images", "MenuItem", item.ImagePath);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    var ext = Path.GetExtension(imageFile.FileName);
                    var fileName = $"{item.MenuItemId}{ext}";
                    var path = Path.Combine(_env.WebRootPath, "images", "MenuItem", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    item.ImagePath = fileName;
                }
                else
                {
                    item.ImagePath = Request.Form["ImagePath"];
                }

                _context.SaveChanges();
                TempData["Success"] = "Cập nhật món ăn thành công!";
                return RedirectToAction("Items", "Menu", new { id = item.MenuCategoryId });
            }
            ViewBag.MenuCategoryId = model.MenuCategoryId;
            ViewBag.CategoryName = _context.MenuCategories
                .FirstOrDefault(c => c.MenuCategoryId == model.MenuCategoryId)?.Name;
            return View(model);
        }

        public IActionResult Details(int id)
        {
            var item = _context.MenuItems
                .Include(m => m.MenuCategory)
                .FirstOrDefault(m => m.MenuItemId == id);
            if (item == null) return NotFound();

            return View(item);
        }

        public IActionResult Delete(int id)
        {
            var item = _context.MenuItems.Find(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _context.MenuItems.Find(id);
            if (item != null)
            {
                // Xoá ảnh nếu có
                if (!string.IsNullOrEmpty(item.ImagePath))
                {
                    var path = Path.Combine(_env.WebRootPath, "images", "MenuItem", item.ImagePath);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                // Xoá nguyên liệu liên quan
                var usages = _context.InventoryUsages.Where(u => u.MenuItemId == item.MenuItemId).ToList();
                _context.InventoryUsages.RemoveRange(usages);

                int catId = item.MenuCategoryId;
                _context.MenuItems.Remove(item);
                _context.SaveChanges();

                TempData["Success"] = "Xoá món ăn và nguyên liệu thành công!";
                return RedirectToAction("Items", "Menu", new { id = catId });
            }
            return RedirectToAction("Index", "Menu");
        }
    }
}
