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
            ViewBag.Tables = new SelectList(_context.DingningTables, "DingningTableId", "Name");
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
                TempData["Success"] = "Tạo đặt bàn thành công.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = new SelectList(_context.Customers, "CustomerId", "Name", reservation.CustomerId);
            ViewBag.Tables = new SelectList(_context.DingningTables, "DingningTableId", "Name", reservation.DingningTableId);
            TempData["Error"] = "Tạo đặt bàn thất bại. Vui lòng kiểm tra lại.";
            return View(reservation);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Không tìm thấy đặt bàn.";
                return RedirectToAction(nameof(Index));
            }

            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.DingningTable)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                TempData["Error"] = "Không tìm thấy đặt bàn.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = new SelectList(_context.Customers, "CustomerId", "Name", reservation.CustomerId);
            ViewBag.Tables = new SelectList(_context.DingningTables, "DingningTableId", "Name", reservation.DingningTableId);
            return View(reservation);
        }

        // POST: Admin/Reservation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Reservation reservation)
        {
            if (id != reservation.ReservationId)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật đặt bàn thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Reservations.Any(e => e.ReservationId == reservation.ReservationId))
                    {
                        TempData["Error"] = "Đặt bàn không tồn tại.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Error"] = "Đã xảy ra lỗi khi cập nhật.";
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState không hợp lệ, load lại danh sách
            ViewBag.Customers = new SelectList(_context.Customers, "CustomerId", "Name", reservation.CustomerId);
            ViewBag.Tables = new SelectList(_context.DingningTables, "DingningTableId", "Name", reservation.DingningTableId);
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
