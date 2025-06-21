using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Helpers;
using RestaurantManagement.Models;
using Models = RestaurantManagement.Models;


namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, string phoneFilter, string vipFilter)
        {
            var customers = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                customers = customers.Where(c => c.Name.Contains(search));

            if (!string.IsNullOrEmpty(phoneFilter))
                customers = customers.Where(c => c.Phone.Contains(phoneFilter));

            if (!string.IsNullOrEmpty(vipFilter))
            {
                bool isVip = vipFilter == "VIP";
                customers = customers.Where(c => c.IsVIP == isVip);
            }

            ViewBag.Search = search;
            ViewBag.PhoneFilter = phoneFilter;
            ViewBag.VipFilter = vipFilter;

            return View(customers.ToList());
        }

        public IActionResult Details(int id)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Models.Customer customer)
        {
            string username = Request.Form["Username"];
            string password = Request.Form["Password"];

            if (ModelState.IsValid)
            {
                if (_context.Accounts.Any(a => a.Username == username))
                {
                    TempData["Error"] = "Tên đăng nhập đã tồn tại.";
                    return View(customer);
                }

                var account = new Account
                {
                    Username = username,
                    Password = HashHelper.ComputeSha256Hash(password),
                    Role = "Customer"
                };

                customer.Username = username;

                _context.Accounts.Add(account);
                _context.Customers.Add(customer);
                _context.SaveChanges();

                TempData["Success"] = "Đã tạo khách hàng mới.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Dữ liệu không hợp lệ.";
            return View(customer);
        }

        public IActionResult Edit(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy khách hàng để sửa.";
                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        [HttpPost]
        public IActionResult Edit(int id, Models.Customer customer)
        {
            if (id != customer.CustomerId)
            {
                TempData["Error"] = "ID không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                _context.Update(customer);
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật khách hàng thành công.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Cập nhật không hợp lệ.";
            return View(customer);
        }

        public IActionResult Delete(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy khách hàng để xoá.";
                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                _context.SaveChanges();
                TempData["Success"] = "Đã xoá khách hàng.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy khách hàng để xoá.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
