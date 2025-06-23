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
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy StaffId từ Claims (chắc chắn hơn là Username)
        private int? GetStaffId()
        {
            var staffIdStr = User.FindFirst("StaffId")?.Value;
            return int.TryParse(staffIdStr, out int id) ? id : null;
        }

        // Danh sách lịch làm việc cá nhân
        public IActionResult Index()
        {
            var staffId = GetStaffId();
            if (staffId == null) return Unauthorized();

            var monday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (DateTime.Today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
            var sunday = monday.AddDays(6);

            var attendances = _context.Attendances
                .Include(a => a.Shift)
                .Where(a => a.StaffId == staffId && a.Date >= monday && a.Date <= sunday)
                .ToList();

            ViewBag.Shifts = _context.Shifts.OrderBy(s => s.StartTime).ToList();

            return View(attendances);
        }

        // Hiển thị form đăng ký ca làm
        // GET: Staff/Schedule/Register
        public IActionResult Register(int weekOffset = 1)
        {
            var staffId = GetStaffId();
            if (staffId == null) return Unauthorized();

            var startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1).AddDays(weekOffset * 7); // Thứ 2 tuần offset
            var endDate = startDate.AddDays(6);

            ViewBag.WeekStart = startDate;
            ViewBag.WeekEnd = endDate;
            ViewBag.Shifts = _context.Shifts.OrderBy(s => s.StartTime).ToList();

            // Lấy các đăng ký hiện tại của nhân viên trong tuần đã chọn
            var registered = _context.Attendances
                .Where(a => a.StaffId == staffId && a.Date >= startDate && a.Date <= endDate)
                .ToList();

            ViewBag.Registered = registered;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string SelectedShiftsJson)
        {
            var staffId = GetStaffId();
            if (staffId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(SelectedShiftsJson))
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một ca làm để đăng ký.";
                return RedirectToAction(nameof(Register));
            }

            var selected = System.Text.Json.JsonSerializer.Deserialize<List<ShiftSelection>>(SelectedShiftsJson);

            if (selected == null || !selected.Any())
            {
                TempData["Error"] = "Lỗi định dạng dữ liệu.";
                return RedirectToAction(nameof(Register));
            }

            foreach (var item in selected)
            {
                if (!DateTime.TryParse(item.date, out DateTime date) || !int.TryParse(item.shiftId, out int shiftId))
                    continue;

                // Kiểm tra trùng lịch
                bool exists = _context.Attendances.Any(a =>
                    a.StaffId == staffId &&
                    a.Date == date &&
                    a.ShiftId == shiftId
                );

                if (!exists)
                {
                    _context.Attendances.Add(new Attendance
                    {
                        StaffId = staffId.Value,
                        ShiftId = shiftId,
                        Date = date,
                        IsPresent = false
                    });
                }
            }

            _context.SaveChanges();
            TempData["Success"] = "Đăng ký ca làm tuần sau thành công!";
            return RedirectToAction(nameof(Index));
        }

        public class ShiftSelection
        {
            public string shiftId { get; set; }
            public string date { get; set; }
        }

        // Cho phép hủy lịch chưa diễn ra
        public IActionResult Cancel(int id)
        {
            var staffId = GetStaffId();
            if (staffId == null) return Unauthorized();

            var attendance = _context.Attendances.FirstOrDefault(a => a.AttendanceId == id && a.StaffId == staffId);
            if (attendance == null || attendance.Date < DateTime.Today)
            {
                TempData["Error"] = "Không thể huỷ ca đã diễn ra hoặc không tồn tại.";
                return RedirectToAction(nameof(Register));
            }

            _context.Attendances.Remove(attendance);
            _context.SaveChanges();
            TempData["Success"] = "Đã huỷ lịch làm việc.";
            return RedirectToAction(nameof(Register));
        }

        [HttpPost]
        public IActionResult SendRequest([FromBody] RequestModel model)
        {
            var staffId = GetStaffId();
            if (staffId == null) return Unauthorized();

            var attendance = _context.Attendances
                .Include(a => a.Staff)
                .FirstOrDefault(a => a.AttendanceId == model.AttendanceId && a.StaffId == staffId);

            if (attendance == null || !attendance.IsApproved)
                return Json(new { success = false });

            var username = attendance.Staff?.Username ?? User.Identity.Name;

            var notification = new Notification
            {
                Title = $"[YÊU CẦU] Thay đổi ca làm: {attendance.Shift?.Name ?? "Ca"} - {attendance.Date:dd/MM/yyyy}",
                Content = model.Content,
                RecipientUsername = "admin", // bạn có thể thay bằng username thật của admin
                Role = "Admin",
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        public class RequestModel
        {
            public int AttendanceId { get; set; }
            public string Content { get; set; }
        }

    }
}
