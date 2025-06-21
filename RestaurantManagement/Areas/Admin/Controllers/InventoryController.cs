using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public InventoryController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(string search, string unitFilter)
        {
            var query = _context.InventoryItems.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i => i.Name.Contains(search));
            }

            if (!string.IsNullOrEmpty(unitFilter))
            {
                query = query.Where(i => i.Unit == unitFilter);
            }

            ViewBag.UnitList = await _context.InventoryItems
                .Select(i => i.Unit)
                .Distinct()
                .ToListAsync();

            return View(await query.OrderBy(i => i.Name).ToListAsync());
        }

        public IActionResult Details(int id)
        {
            var item = _context.InventoryItems.FirstOrDefault(i => i.InventoryItemId == id);
            if (item == null)
            {
                TempData["Error"] = "Không tìm thấy nguyên liệu.";
                return RedirectToAction("Index");
            }
            return View(item);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(InventoryItem item, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                _context.InventoryItems.Add(item);
                _context.SaveChanges();

                // Lưu ảnh
                if (image != null)
                {
                    var ext = Path.GetExtension(image.FileName);
                    var path = Path.Combine(_env.WebRootPath, "images", "InventoryItem");
                    Directory.CreateDirectory(path);
                    var filePath = Path.Combine(path, $"{item.InventoryItemId}{ext}");

                    using var stream = new FileStream(filePath, FileMode.Create);
                    image.CopyTo(stream);

                    item.ImagePath = $"/images/InventoryItem/{item.InventoryItemId}{ext}";
                    _context.Update(item);
                    _context.SaveChanges();
                }

                TempData["Success"] = "Đã thêm nguyên liệu mới.";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Lỗi khi thêm nguyên liệu.";
            return View(item);
        }

        public IActionResult Edit(int id)
        {
            var item = _context.InventoryItems.Find(id);
            if (item == null)
            {
                TempData["Error"] = "Không tìm thấy nguyên liệu.";
                return RedirectToAction("Index");
            }
            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(InventoryItem model, IFormFile? imageFile)
        {
            var item = _context.InventoryItems.FirstOrDefault(i => i.InventoryItemId == model.InventoryItemId);
            if (item == null) return NotFound();

            if (ModelState.IsValid)
            {
                item.Name = model.Name;
                item.Quantity = model.Quantity;
                item.Unit = model.Unit;

                if (imageFile != null && imageFile.Length > 0)
                {
                    // Xoá ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(item.ImagePath))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, "images", "InventoryItem", Path.GetFileName(item.ImagePath));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    // Lưu ảnh mới
                    var ext = Path.GetExtension(imageFile.FileName);
                    var fileName = $"{item.InventoryItemId}{ext}";
                    var path = Path.Combine(_env.WebRootPath, "images", "InventoryItem", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    item.ImagePath = "/images/InventoryItem/" + fileName;
                }

                _context.SaveChanges();
                TempData["Success"] = "Cập nhật nguyên liệu thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }



        public IActionResult Delete(int id)
        {
            var item = _context.InventoryItems.Find(id);
            if (item == null)
            {
                TempData["Error"] = "Không tìm thấy nguyên liệu.";
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _context.InventoryItems.FirstOrDefault(i => i.InventoryItemId == id);
            if (item == null)
            {
                TempData["Error"] = "Không tìm thấy nguyên liệu để xoá!";
                return RedirectToAction(nameof(Index));
            }

            // Xoá ảnh nếu có
            if (!string.IsNullOrEmpty(item.ImagePath))
            {
                var filePath = Path.Combine(_env.WebRootPath, item.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.InventoryItems.Remove(item);
            _context.SaveChanges();
            TempData["Success"] = "Đã xoá nguyên liệu thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
