using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class InventoryTransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryTransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchType, int? itemId)
        {
            var query = _context.InventoryTransactions.Include(t => t.InventoryItem).AsQueryable();

            if (!string.IsNullOrEmpty(searchType))
            {
                query = query.Where(t => t.Type.Contains(searchType));
            }

            if (itemId.HasValue)
            {
                query = query.Where(t => t.InventoryItemId == itemId.Value);
            }

            ViewBag.Items = new SelectList(_context.InventoryItems, "InventoryItemId", "Name");
            return View(await query.OrderByDescending(t => t.CreatedAt).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewBag.InventoryItemId = new SelectList(_context.InventoryItems, "InventoryItemId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryTransaction model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                _context.Add(model);

                var item = await _context.InventoryItems.FindAsync(model.InventoryItemId);
                if (item != null)
                {
                    switch (model.Type.ToLower())
                    {
                        case "nhập":
                            item.Quantity += model.Quantity;
                            break;
                        case "xuất":
                        case "hủy":
                            item.Quantity -= model.Quantity;
                            break;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã tạo giao dịch kho thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.InventoryItemId = new SelectList(_context.InventoryItems, "InventoryItemId", "Name", model.InventoryItemId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _context.InventoryTransactions.FindAsync(id);
            if (transaction == null) return NotFound();

            ViewBag.InventoryItemId = new SelectList(_context.InventoryItems, "InventoryItemId", "Name", transaction.InventoryItemId);
            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InventoryTransaction model)
        {
            if (id != model.InventoryTransactionId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var oldTransaction = await _context.InventoryTransactions.AsNoTracking().FirstOrDefaultAsync(t => t.InventoryTransactionId == id);
                    var item = await _context.InventoryItems.FindAsync(model.InventoryItemId);

                    // Rollback old transaction
                    if (item != null && oldTransaction != null)
                    {
                        switch (oldTransaction.Type.ToLower())
                        {
                            case "nhập":
                                item.Quantity -= oldTransaction.Quantity;
                                break;
                            case "xuất":
                            case "hủy":
                                item.Quantity += oldTransaction.Quantity;
                                break;
                        }

                        // Apply new transaction
                        switch (model.Type.ToLower())
                        {
                            case "nhập":
                                item.Quantity += model.Quantity;
                                break;
                            case "xuất":
                            case "hủy":
                                item.Quantity -= model.Quantity;
                                break;
                        }
                    }

                    model.CreatedAt = oldTransaction.CreatedAt; // giữ nguyên thời điểm tạo
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đã cập nhật giao dịch thành công!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Lỗi khi cập nhật: {ex.Message}";
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.InventoryItemId = new SelectList(_context.InventoryItems, "InventoryItemId", "Name", model.InventoryItemId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _context.InventoryTransactions
                .Include(t => t.InventoryItem)
                .FirstOrDefaultAsync(m => m.InventoryTransactionId == id);
            if (transaction == null) return NotFound();

            return View(transaction);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.InventoryTransactions.FindAsync(id);
            if (transaction == null) return NotFound();

            var item = await _context.InventoryItems.FindAsync(transaction.InventoryItemId);
            if (item != null)
            {
                switch (transaction.Type.ToLower())
                {
                    case "nhập":
                        item.Quantity -= transaction.Quantity;
                        break;
                    case "xuất":
                    case "hủy":
                        item.Quantity += transaction.Quantity;
                        break;
                }
            }

            _context.InventoryTransactions.Remove(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xoá giao dịch kho thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _context.InventoryTransactions
                .Include(t => t.InventoryItem)
                .FirstOrDefaultAsync(m => m.InventoryTransactionId == id);

            if (transaction == null) return NotFound();
            return View(transaction);
        }
    }
}
