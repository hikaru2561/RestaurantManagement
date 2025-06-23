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
    public class TableController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TableController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, TableStatus? statusFilter, string vipFilter)
        {
            var tables = _context.DingningTables.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                tables = tables.Where(t => t.Name.Contains(search));

            if (statusFilter.HasValue)
                tables = tables.Where(t => t.Status == statusFilter.Value);

            if (bool.TryParse(vipFilter, out var vipBool))
                tables = tables.Where(t => t.IsVIP == vipBool);

            return View(tables.ToList());
        }

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(Table model)
        {
            if (ModelState.IsValid)
            {
                model.Status = TableStatus.Available;
                _context.DingningTables.Add(model);
                _context.SaveChanges();
                TempData["Success"] = "Thêm bàn thành công!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var table = _context.DingningTables.Find(id);
            if (table == null) return NotFound();
            return View(table);
        }

        [HttpPost]
        public IActionResult Edit(Table model)
        {
            if (ModelState.IsValid)
            {
                _context.DingningTables.Update(model);
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật bàn thành công!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult Details(int id)
        {
            var table = _context.DingningTables.Find(id);
            if (table == null) return NotFound();
            return View(table);
        }

        public IActionResult Delete(int id)
        {
            var table = _context.DingningTables.Include(t => t.Orders).Include(t => t.Reservations).FirstOrDefault(t => t.TableId == id);
            if (table == null) return NotFound();
            return View(table);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var table = _context.DingningTables
                .Include(t => t.Orders)
                .Include(t => t.Reservations)
                .FirstOrDefault(t => t.TableId == id);

            if (table == null)
                return NotFound();

            bool hasOrders = table.Orders?.Any() == true;
            bool hasReservations = table.Reservations?.Any() == true;

            if (hasOrders || hasReservations)
            {
                TempData["Error"] = "Không thể xoá bàn vì đang có đơn hàng hoặc đặt bàn liên quan.";
                return RedirectToAction("Index");
            }

            _context.DingningTables.Remove(table);
            _context.SaveChanges();

            TempData["Success"] = "Xoá bàn thành công!";
            return RedirectToAction("Index");
        }
    }
}
