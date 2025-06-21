using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Data;
using System.Linq;
using System.Security.Claims;

namespace RestaurantManagement.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var username = User.Identity.Name;

            // Lấy thông tin nhân viên hiện tại
            var staff = _context.Staffs.FirstOrDefault(s => s.Username == username);
            if (staff == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin nhân viên.";
                return RedirectToAction("Logout", "Account", new { area = "" });
            }

            // Truyền thông tin xuống ViewBag
            ViewBag.StaffName = staff.Name;

            // Thống kê đơn hàng hôm nay
            var today = DateTime.Today;
            ViewBag.TodayOrders = _context.Orders
                .Where(o => o.OrderTime.Date == today && o.StaffId == staff.StaffId)
                .Count();

            // Lượt chấm công gần nhất
            var lastAttendance = _context.Attendances
                .Where(a => a.StaffId == staff.StaffId)
                .OrderByDescending(a => a.Date)
                .FirstOrDefault();
            ViewBag.LastAttendance = lastAttendance;

            return View();
        }
    }
}
