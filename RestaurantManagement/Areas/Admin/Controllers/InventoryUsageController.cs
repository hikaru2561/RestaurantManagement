using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class InventoryUsageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryUsageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Xem danh sách nguyên liệu của 1 món
        public async Task<IActionResult> Index(int menuItemId)
        {
            var menuItem = await _context.MenuItems.FindAsync(menuItemId);
            if (menuItem == null)
            {
                TempData["Error"] = "Món ăn không tồn tại!";
                return RedirectToAction("Index", "MenuItem");
            }

            ViewBag.MenuItemName = menuItem.Name;
            ViewBag.MenuItemId = menuItemId;
            ViewBag.MenuCategoryId = menuItem.MenuCategoryId; 
            var usages = await _context.InventoryUsages
                .Include(u => u.InventoryItem)
                .Where(u => u.MenuItemId == menuItemId)
                .ToListAsync();

            return View(usages);
        }

        // Hiển thị form tạo mới nguyên liệu sử dụng
        public IActionResult Create(int menuItemId)
        {
            var menuItem = _context.MenuItems.FirstOrDefault(m => m.MenuItemId == menuItemId);
            if (menuItem == null)
            {
                TempData["Error"] = "Món ăn không tồn tại!";
                return RedirectToAction("Index", "MenuItem");
            }

            ViewBag.MenuItemName = menuItem.Name;
            ViewBag.MenuItemId = menuItemId;
            ViewBag.MenuCategoryId = menuItem.MenuCategoryId;
            ViewBag.InventoryItemId = new SelectList(_context.InventoryItems, "InventoryItemId", "Name");

            return View(new InventoryUsage { MenuItemId = menuItemId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryUsage usage)
        {
            if (ModelState.IsValid)
            {
                // KIỂM TRA: nguyên liệu này đã được thêm vào món ăn chưa
                bool isDuplicate = await _context.InventoryUsages
                    .AnyAsync(u => u.MenuItemId == usage.MenuItemId && u.InventoryItemId == usage.InventoryItemId);

                if (isDuplicate)
                {
                    TempData["Error"] = "Nguyên liệu này đã được thêm vào món ăn!";
                    return RedirectToAction(nameof(Index), new { menuItemId = usage.MenuItemId });
                }

                // THÊM MỚI nếu không trùng
                _context.InventoryUsages.Add(usage);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã thêm nguyên liệu vào món ăn!";
                return RedirectToAction(nameof(Index), new { menuItemId = usage.MenuItemId });
            }

            ViewBag.InventoryItemId = new SelectList(_context.InventoryItems, "InventoryItemId", "Name", usage.InventoryItemId);
            ViewBag.MenuItemId = usage.MenuItemId;
            return View(usage);
        }

        // Xem chi tiết nguyên liệu sử dụng
        public async Task<IActionResult> Details(int id)
        {
            var usage = await _context.InventoryUsages
                .Include(u => u.InventoryItem)
                .Include(u => u.MenuItem)
                .FirstOrDefaultAsync(u => u.InventoryUsageId == id);

            if (usage == null)
            {
                TempData["Error"] = "Không tìm thấy nguyên liệu sử dụng!";
                return RedirectToAction("Index", "MenuItem");
            }

            return View(usage);
        }

        // Chỉnh sửa nguyên liệu sử dụng
        public async Task<IActionResult> Edit(int id)
        {
            var usage = await _context.InventoryUsages.FindAsync(id);
            if (usage == null)
            {
                TempData["Error"] = "Không tìm thấy dữ liệu để sửa!";
                return RedirectToAction("Index", "MenuItem");
            }

            ViewBag.InventoryItemId = new SelectList(_context.InventoryItems, "InventoryItemId", "Name", usage.InventoryItemId);
            ViewBag.MenuItemId = usage.MenuItemId;

            return View(usage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InventoryUsage usage)
        {
            if (id != usage.InventoryUsageId)
            {
                TempData["Error"] = "ID không hợp lệ!";
                return RedirectToAction("Index", "MenuItem");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usage);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đã cập nhật nguyên liệu sử dụng!";
                    return RedirectToAction(nameof(Index), new { menuItemId = usage.MenuItemId });
                }
                catch
                {
                    TempData["Error"] = "Có lỗi khi cập nhật!";
                }
            }

            ViewBag.InventoryItemId = new SelectList(_context.InventoryItems, "InventoryItemId", "Name", usage.InventoryItemId);
            ViewBag.MenuItemId = usage.MenuItemId;
            return View(usage);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var usage = await _context.InventoryUsages
                .Include(u => u.InventoryItem)
                .Include(u => u.MenuItem)
                .FirstOrDefaultAsync(u => u.InventoryUsageId == id);

            if (usage == null)
            {
                TempData["Error"] = "Không tìm thấy nguyên liệu sử dụng!";
                return RedirectToAction("Index", "MenuItem");
            }

            return View(usage);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usage = await _context.InventoryUsages.FindAsync(id);
            if (usage != null)
            {
                int menuItemId = usage.MenuItemId;
                _context.InventoryUsages.Remove(usage);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xoá nguyên liệu khỏi món ăn!";
                return RedirectToAction(nameof(Index), new { menuItemId });
            }

            TempData["Error"] = "Không thể xoá nguyên liệu!";
            return RedirectToAction("Index", "MenuItem");
        }
    }
}
