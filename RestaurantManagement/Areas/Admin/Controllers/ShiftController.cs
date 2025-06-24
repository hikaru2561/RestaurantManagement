using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ShiftController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShiftController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Shifts
        public async Task<IActionResult> Index()
        {
            var shifts = await _context.Shifts
                .Include(s => s.Attendances)
                .ToListAsync();
            return View(shifts);
        }

        // GET: Shifts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Shifts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shift shift)
        {
            if (ModelState.IsValid)
            {
                _context.Add(shift);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã thêm ca làm mới.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "Không thể tạo ca làm.";
            return View(shift);
        }

        // GET: Shifts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null) return NotFound();
            return View(shift);
        }

        // POST: Shifts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Shift shift)
        {
            if (id != shift.ShiftId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shift);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đã cập nhật ca làm.";
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật.";
                }
            }
            return View(shift);
        }

        // GET: Shifts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var shift = await _context.Shifts
                .Include(s => s.Attendances)
                .ThenInclude(a => a.Staff)
                .FirstOrDefaultAsync(m => m.ShiftId == id);
            if (shift == null) return NotFound();
            return View(shift);
        }

        // GET: Shifts/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null) return NotFound();
            return View(shift);
        }

        // POST: Shifts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift != null)
            {
                _context.Shifts.Remove(shift);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xoá ca làm.";
            }
            else
            {
                TempData["Error"] = "Ca làm không tồn tại.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Shifts/ManageAttendance/5?date=yyyy-MM-dd
        public async Task<IActionResult> ManageAttendance(int id, DateTime? date)
        {
            var shift = await _context.Shifts
                .Include(s => s.Attendances)
                .ThenInclude(a => a.Staff)
                .FirstOrDefaultAsync(s => s.ShiftId == id);
            if (shift == null) return NotFound();

            var staffs = await _context.Staffs.ToListAsync();
            var targetDate = date ?? DateTime.Today;

            var attendanceDict = await _context.Attendances
                .Where(a => a.ShiftId == id && a.Date.Date == targetDate.Date)
                .ToDictionaryAsync(a => a.StaffId, a => a);

            ViewBag.Staffs = staffs;
            ViewBag.Date = targetDate;
            ViewBag.Shift = shift;

            return View(attendanceDict);
        }

        // POST: Shifts/ManageAttendance/5
        // POST: Shifts/ManageAttendance/5
        [HttpPost]
        public async Task<IActionResult> ManageAttendance(int id, DateTime date, List<int> presentStaffIds, List<int> absentStaffIds)
        {
            var allStaff = await _context.Staffs.ToListAsync();

            foreach (var staff in allStaff)
            {
                var existing = await _context.Attendances.FirstOrDefaultAsync(a =>
                    a.ShiftId == id && a.Date.Date == date.Date && a.StaffId == staff.StaffId);

                if (presentStaffIds.Contains(staff.StaffId))
                {
                    if (existing != null)
                    {
                        existing.Status = AttendanceStatus.Present;
                        _context.Update(existing);
                    }
                    else
                    {
                        _context.Add(new Attendance
                        {
                            ShiftId = id,
                            StaffId = staff.StaffId,
                            Date = date,
                            Status = AttendanceStatus.Present
                        });
                    }
                }
                else if (absentStaffIds.Contains(staff.StaffId))
                {
                    if (existing != null)
                    {
                        existing.Status = AttendanceStatus.Absent;
                        _context.Update(existing);
                    }
                    else
                    {
                        _context.Add(new Attendance
                        {
                            ShiftId = id,
                            StaffId = staff.StaffId,
                            Date = date,
                            Status = AttendanceStatus.Absent
                        });
                    }
                }
                else if (existing == null)
                {
                    // Nếu không có trong 2 danh sách nhưng chưa tồn tại: tạo trạng thái Đã duyệt
                    _context.Add(new Attendance
                    {
                        ShiftId = id,
                        StaffId = staff.StaffId,
                        Date = date,
                        Status = AttendanceStatus.Approved
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật chấm công.";
            return RedirectToAction(nameof(Details), new { id });
        }

    }
}
