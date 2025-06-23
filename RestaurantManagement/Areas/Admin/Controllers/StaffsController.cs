using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using RestaurantManagement.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StaffsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StaffsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Staffs
        public async Task<IActionResult> Index(string search)
        {
            var staffs = from s in _context.Staffs select s;

            if (!string.IsNullOrEmpty(search))
            {
                staffs = staffs.Where(s => s.Name.Contains(search) || s.Phone.Contains(search));
            }

            ViewBag.Search = search;
            return View(await staffs.ToListAsync());
        }

        // GET: Staffs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var staff = await _context.Staffs.FirstOrDefaultAsync(m => m.StaffId == id);
            if (staff == null) return NotFound();

            return View(staff);
        }

        // GET: Staffs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Staffs/Create
        [HttpPost]
        public async Task<IActionResult> Create(Staffs staff, IFormFile imageFile)
        {
            string username = Request.Form["Username"];
            string password = Request.Form["Password"];

            if (ModelState.IsValid)
            {
                staff.ImagePath = "";
                _context.Staffs.Add(staff);
                await _context.SaveChangesAsync();

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
                    _context.Update(staff);
                    await _context.SaveChangesAsync();
                }

                var hashedPassword = HashHelper.ComputeSha256Hash(password);
                var account = new Account
                {
                    Username = username,
                    Password = hashedPassword,
                    Role = "Staff",
                };
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã thêm nhân viên và tài khoản thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        // GET: Admin/Staffs/Edit/5
        public IActionResult Edit(int id)
        {
            var staff = _context.Staffs.FirstOrDefault(s => s.StaffId == id);
            if (staff == null)
            {
                TempData["Error"] = "Không tìm thấy nhân viên.";
                return RedirectToAction("Index");
            }
            return View(staff);
        }

        // POST: Admin/Staffs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Staffs model, IFormFile? imageFile)
        {
            var staff = _context.Staffs.FirstOrDefault(s => s.StaffId == model.StaffId);
            if (staff == null)
            {
                TempData["Error"] = "Không tìm thấy nhân viên cần cập nhật.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật thông tin cơ bản
                    staff.Name = model.Name;
                    staff.Phone = model.Phone;
                    staff.Username = model.Username;

                    // Nếu có ảnh mới
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var ext = Path.GetExtension(imageFile.FileName);
                        var fileName = $"{staff.StaffId}{ext}";
                        var folder = Path.Combine(_env.WebRootPath, "images", "Staffs");
                        Directory.CreateDirectory(folder);

                        // Xoá ảnh cũ nếu tồn tại
                        if (!string.IsNullOrEmpty(staff.ImagePath))
                        {
                            var oldImagePath = Path.Combine(folder, staff.ImagePath);
                            if (System.IO.File.Exists(oldImagePath))
                                System.IO.File.Delete(oldImagePath);
                        }

                        // Lưu ảnh mới
                        var fullPath = Path.Combine(folder, fileName);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        // Cập nhật đường dẫn ảnh
                        staff.ImagePath = fileName;
                    }

                    _context.Staffs.Update(staff);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật nhân viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Đã xảy ra lỗi khi cập nhật: {ex.Message}";
                }
            }

            return View(model);
        }



        // GET: Staffs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var staff = await _context.Staffs.FirstOrDefaultAsync(m => m.StaffId == id);
            if (staff == null) return NotFound();

            return View(staff);
        }

        // POST: Staffs/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            if (staff != null)
            {
                _context.Staffs.Remove(staff);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa nhân viên thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
