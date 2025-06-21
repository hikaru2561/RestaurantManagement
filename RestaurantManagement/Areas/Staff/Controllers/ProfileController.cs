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
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProfileController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private Staffs? GetCurrentStaff()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            return _context.Staffs.FirstOrDefault(s => s.Username == username);
        }

        public IActionResult Index()
        {
            var staff = GetCurrentStaff();
            if (staff == null) return Unauthorized();
            return View(staff);
        }

        public IActionResult Edit()
        {
            var staff = GetCurrentStaff();
            if (staff == null) return Unauthorized();
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int StaffId, Staffs model, IFormFile? imageFile)
        {
            var staff = await _context.Staffs.FindAsync(StaffId);
            if (staff == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật chỉ các trường được phép sửa
                    staff.Name = model.Name;
                    staff.Phone = model.Phone;

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var ext = Path.GetExtension(imageFile.FileName);
                        var fileName = $"{staff.StaffId}{ext}";
                        var folder = Path.Combine(_env.WebRootPath, "images", "Staffs");
                        Directory.CreateDirectory(folder);
                        var path = Path.Combine(folder, fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        staff.ImagePath = fileName;
                    }

                    // Không cập nhật các trường khác (Username, Role, v.v...)

                    _context.Update(staff);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Staffs.Any(e => e.StaffId == model.StaffId))
                        return NotFound();
                    else
                        throw;
                }
            }

            TempData["Error"] = "Dữ liệu không hợp lệ.";
            return View(model);
        }
    }
}
