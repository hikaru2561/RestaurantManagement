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

        // GET: Staffs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null) return NotFound();

            return View(staff);
        }

        // POST: Staffs/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Staffs staff, IFormFile imageFile)
        {
            if (id != staff.StaffId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
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

                    _context.Update(staff);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật nhân viên thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Staffs.Any(e => e.StaffId == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
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
