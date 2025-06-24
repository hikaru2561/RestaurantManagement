using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using Microsoft.EntityFrameworkCore;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AttendanceController : Controller
{
    private readonly ApplicationDbContext _context;

    public AttendanceController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(int weekOffset = 0)
    {
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1 + weekOffset * 7);
        var endOfWeek = startOfWeek.AddDays(6);

        var shifts = _context.Shifts.OrderBy(s => s.StartTime).ToList();
        var attendances = _context.Attendances
            .Include(a => a.Staff)
            .Include(a => a.Shift)
            .Where(a => a.Date >= startOfWeek && a.Date <= endOfWeek)
            .ToList();

        ViewBag.StartOfWeek = startOfWeek;
        ViewBag.EndOfWeek = endOfWeek;
        ViewBag.WeekOffset = weekOffset;
        ViewBag.Shifts = shifts;

        return View(attendances);
    }

    [HttpGet]
    public IActionResult GetRegistrations(int shiftId, string date)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
            return BadRequest();

        var registrations = _context.Attendances
            .Include(a => a.Staff)
            .Where(a => a.ShiftId == shiftId && a.Date.Date == parsedDate.Date)
            .ToList();

        return PartialView("_ApproveModal", registrations);
    }

    [HttpPost]
    public IActionResult ApproveSelected([FromBody] List<int> ids)
    {
        if (ids == null || ids.Count == 0)
            return Json(new { success = false });

        var attendances = _context.Attendances
            .Where(a => ids.Contains(a.AttendanceId))
            .ToList();

        foreach (var att in attendances)
        {
            if (att.Status == AttendanceStatus.Registered)
                att.Status = AttendanceStatus.Approved;
        }

        _context.SaveChanges();
        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult CancelAttendance(int id)
    {
        var att = _context.Attendances.Include(a => a.Staff).FirstOrDefault(a => a.AttendanceId == id);
        if (att == null)
        {
            TempData["Error"] = "Không tìm thấy lịch làm việc.";
            return RedirectToAction(nameof(Index));
        }

        _context.Attendances.Remove(att);
        _context.SaveChanges();
        TempData["Success"] = $"Đã huỷ lịch làm việc cho {att.Staff?.Name} vào {att.Date:dd/MM}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult GetCurrentShiftAttendances()
    {
        var now = DateTime.Now.TimeOfDay;
        var today = DateTime.Today;

        var shiftNow = _context.Shifts.AsEnumerable()
            .FirstOrDefault(s => s.StartTime.TimeOfDay <= now && s.EndTime.TimeOfDay >= now);

        if (shiftNow == null)
            return PartialView("_CurrentShift", new List<Attendance>());

        var attendances = _context.Attendances
            .Include(a => a.Staff)
            .Where(a => a.Date.Date == today &&
                        a.ShiftId == shiftNow.ShiftId &&
                        a.Status == AttendanceStatus.Approved)
            .ToList();

        ViewBag.CurrentShift = shiftNow;
        return PartialView("_CurrentShift", attendances);
    }

    [HttpPost]
    public IActionResult MarkPresent([FromBody] MarkPresentDto data)
    {
        var att = _context.Attendances.FirstOrDefault(a => a.AttendanceId == data.Id);
        if (att == null)
            return Json(new { success = false, message = "Không tìm thấy dữ liệu." });

        att.Status = AttendanceStatus.Present;
        _context.SaveChanges();
        return Json(new { success = true });
    }

    public class MarkPresentDto
    {
        public int Id { get; set; }
    }
}
