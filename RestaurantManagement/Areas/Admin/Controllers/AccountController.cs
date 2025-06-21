using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using System.Linq;
using RestaurantManagement.Helpers;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchString, string roleFilter)
        {
            var accounts = _context.Accounts.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                accounts = accounts.Where(a => a.Username.Contains(searchString));

            if (!string.IsNullOrEmpty(roleFilter))
                accounts = accounts.Where(a => a.Role == roleFilter);

            ViewBag.RoleFilter = roleFilter;
            ViewBag.SearchString = searchString;
            ViewBag.Roles = _context.Accounts.Select(a => a.Role).Distinct().ToList();

            return View(accounts.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Models.Account account)
        {
            if (_context.Accounts.Any(a => a.Username == account.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                account.Password = HashHelper.ComputeSha256Hash(account.Password);
                _context.Accounts.Add(account);
                _context.SaveChanges();
                TempData["Success"] = "Tạo tài khoản thành công.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Lỗi khi tạo tài khoản.";
            return View(account);
        }

        public IActionResult Edit(int id)
        {
            var account = _context.Accounts.Find(id);
            if (account == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        [HttpPost]
        public IActionResult Edit(Models.Account account)
        {
            if (_context.Accounts.Any(a => a.Username == account.Username && a.AccountId != account.AccountId))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                var existing = _context.Accounts.AsNoTracking().FirstOrDefault(a => a.AccountId == account.AccountId);
                if (existing != null && existing.Password != account.Password)
                {
                    account.Password = HashHelper.ComputeSha256Hash(account.Password);
                }

                _context.Update(account);
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật tài khoản thành công.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Lỗi khi cập nhật tài khoản.";
            return View(account);
        }


        public IActionResult Delete(int id)
        {
            var account = _context.Accounts.Find(id);
            if (account == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var account = _context.Accounts.Find(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
                _context.SaveChanges();
                TempData["Success"] = "Xóa tài khoản thành công.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy tài khoản để xóa.";
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var account = _context.Accounts.Find(id);
            if (account == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }
    }
}
