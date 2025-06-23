using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Areas.Staff.Models;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using System.Security.Claims;

namespace RestaurantManagement.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetStaffIdFromClaims()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            return _context.Staffs.FirstOrDefault(s => s.Username == username)?.StaffId;
        }

        public async Task<IActionResult> Index()
        {
            var staffId = GetStaffIdFromClaims();
            if (staffId == null) return Unauthorized();

            var staff = await _context.Staffs.FindAsync(staffId);
            if (staff == null) return NotFound();

            return View(staff);
        }


        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var staffId = GetStaffIdFromClaims();
            if (staffId == null) return Unauthorized();

            var staff = await _context.Staffs.FindAsync(staffId);
            if (staff == null) return NotFound();

            var vm = new EditProfileViewModel
            {
                StaffId = staff.StaffId,
                Name = staff.Name,
                Phone = staff.Phone
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            var staffId = GetStaffIdFromClaims();
            if (staffId == null || staffId != model.StaffId)
            {
                TempData["Error"] = "Không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return View(model); // trả lại view cùng dữ liệu người dùng nhập
            }

            var staff = await _context.Staffs.FindAsync(model.StaffId);
            if (staff == null)
            {
                TempData["Error"] = "Không tìm thấy nhân viên.";
                return RedirectToAction(nameof(Index));
            }

            bool hasChanges = false;

            if (staff.Name != model.Name)
            {
                staff.Name = model.Name;
                hasChanges = true;
            }

            if (staff.Phone != model.Phone)
            {
                staff.Phone = model.Phone;
                hasChanges = true;
            }

            if (hasChanges)
            {
                _context.Update(staff);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thông tin đã được cập nhật.";
            }
            else
            {
                TempData["Success"] = "Không có thay đổi nào.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
