using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestaurantManagement.Controllers.Staff
{
    [Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                TempData["Error"] = "Không xác định được người dùng.";
                return RedirectToAction("Login", "Login");
            }

            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Username == username);

            if (staff == null)
            {
                TempData["Error"] = "Không tìm thấy nhân viên tương ứng.";
                return RedirectToAction("Login", "Login");
            }

            ViewBag.Staff = staff;

            var today = DateTime.Today;

            // Đếm đơn hàng hôm nay
            var orderCount = await _context.Orders
                .CountAsync(o => o.StaffId == staff.StaffId && o.OrderTime.Date == today);

            // Đếm số lịch làm hôm nay
            var attendanceCount = await _context.Attendances
                .CountAsync(a => a.StaffId == staff.StaffId && a.Date == today);

            ViewBag.OrderCount = orderCount;
            ViewBag.AttendanceCount = attendanceCount;

            return View();
        }
    }
}
