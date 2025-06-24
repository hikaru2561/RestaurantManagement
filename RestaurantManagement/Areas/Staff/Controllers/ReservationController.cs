using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using System.Security.Claims;

namespace RestaurantManagement.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff")]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetStaffId()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            return _context.Staffs.FirstOrDefault(s => s.Username == username)?.StaffId ?? 0;
        }

        public IActionResult Index()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var reservations = _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.DingningTable)
                .Where(r => r.ReservationTime >= today
                         && r.ReservationTime < tomorrow
                         && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.ReservationTime)
                .ToList();

            return View(reservations);
        }

        public IActionResult Details(int id)
        {
            var reservation = _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.DingningTable)
                .FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null || reservation.Status != ReservationStatus.Pending)
                return NotFound();

            return View(reservation);
        }

        [HttpPost]
        public IActionResult Confirm(int id)
        {
            var reservation = _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.DingningTable)
                .FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null || reservation.Status != ReservationStatus.Pending)
            {
                TempData["Error"] = "Đặt bàn không hợp lệ.";
                return RedirectToAction("Index");
            }

            // Tạo đơn hàng mới
            var staffId = GetStaffId();
            var order = new Order
            {
                CustomerId = reservation.CustomerId,
                DingningTableId = reservation.DingningTableId,
                OrderTime = DateTime.Now,
                StaffId = staffId,
                Status = OrderStatus.Ordered
            };
            _context.Orders.Add(order);

            // Cập nhật trạng thái đặt bàn
            reservation.Status = ReservationStatus.Confirmed;

            // Cập nhật bàn đang sử dụng
            var table = _context.DingningTables.Find(reservation.DingningTableId);
            if (table != null)
                table.Status = TableStatus.InUse;

            _context.SaveChanges();

            TempData["Success"] = "Khách đã đến, đơn hàng đã được tạo.";
            return RedirectToAction("Details", "Order", new { area = "Staff", id = order.OrderId });
        }
    }
}
