using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Data;
using System.Security.Claims;
using System.Linq;
using RestaurantManagement.Models;

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
            var staffIdStr = User.FindFirst("StaffId")?.Value;
            var staffName = User.FindFirst("FullName")?.Value;

            if (string.IsNullOrEmpty(staffIdStr) || !int.TryParse(staffIdStr, out var staffId))
            {
                TempData["Error"] = "Không xác định được nhân viên.";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }

            var today = DateTime.Today;

            ViewBag.TodayOrders = _context.Orders
                .Count(o => o.StaffId == staffId && o.OrderTime.Date == today);

            ViewBag.TablesInUse = _context.DingningTables
                .Count(t => t.Status == TableStatus.InUse);

            ViewBag.PendingOrders = _context.Orders
                .Count(o => o.StaffId == staffId && o.Status == OrderStatus.Ordered);

            ViewBag.TodayShift = _context.Attendances
                .Where(a => a.StaffId == staffId && a.Date == today)
                .Select(a => a.Shift)
                .FirstOrDefault();

            ViewBag.NextShift = _context.Attendances
                .Where(a => a.StaffId == staffId && a.Date > today)
                .OrderBy(a => a.Date)
                .Select(a => a.Date.ToString("dd/MM/yyyy") + " - Ca " + a.Shift)
                .FirstOrDefault();

            return View();
        }




    }
}
