using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.DingningTable)
                .OrderByDescending(r => r.ReservationTime)
                .ToListAsync();
            return View(reservations);
        }

        public IActionResult Create()
        {
            ViewBag.Customers = new SelectList(_context.Customers, "CustomerId", "Name");
            ViewBag.DingningTables = new SelectList(_context.DingningTables, "TableId", "Name");
            ViewBag.Customers = new SelectList(_context.Customers
    .Select(c => new { c.CustomerId, Name = c.Name + " - " + c.Phone }), "CustomerId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã đặt bàn thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = new SelectList(_context.Customers, "CustomerId", "Name", reservation.CustomerId);
            ViewBag.DingningTables = new SelectList(_context.DingningTables, "TableId", "Name", reservation.DingningTableId);
            return View(reservation);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            ViewBag.Customers = new SelectList(_context.Customers
      .Select(c => new { c.CustomerId, Name = c.Name + " - " + c.Phone }), "CustomerId", "Name");
            ViewBag.DingningTables = new SelectList(_context.DingningTables, "TableId", "Name", reservation.DingningTableId);

            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Reservation reservation)
        {
            if (id != reservation.ReservationId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(reservation);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật đặt bàn thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = new SelectList(_context.Customers, "CustomerId", "Name", reservation.CustomerId);
            ViewBag.DingningTables = new SelectList(_context.DingningTables, "TableId", "Name", reservation.DingningTableId);
            return View(reservation);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.DingningTable)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null) return NotFound();

            return View(reservation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xoá đặt bàn thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
