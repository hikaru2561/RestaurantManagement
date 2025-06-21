using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var nextWeek = DateTime.Today.AddDays(7 - (int)DateTime.Today.DayOfWeek);
            var endOfWeek = nextWeek.AddDays(6);

            var pendingAttendances = _context.Attendances
                .Include(a => a.Staff)
                .Include(a => a.Shift)
                .Where(a => !a.IsApproved && a.Date >= nextWeek && a.Date <= endOfWeek)
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Shift.StartTime)
                .ToList();

            return View(pendingAttendances);
        }

        [HttpPost]
        public IActionResult Approve(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["Error"] = "Không có lịch nào được chọn.";
                return RedirectToAction(nameof(Index));
            }

            var records = _context.Attendances.Where(a => selectedIds.Contains(a.AttendanceId)).ToList();

            foreach (var a in records)
            {
                a.IsApproved = true;
            }

            _context.SaveChanges();
            TempData["Success"] = "Duyệt lịch làm việc thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
