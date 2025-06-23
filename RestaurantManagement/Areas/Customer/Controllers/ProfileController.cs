using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using System.Security.Claims;

namespace RestaurantManagement.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCustomerId()
        {
            var idStr = User.FindFirst("CustomerId")?.Value;
            return int.TryParse(idStr, out var id) ? id : 0;
        }

        public IActionResult Index()
        {
            var customerId = GetCustomerId();
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpGet]
        public IActionResult Edit()
        {
            var customerId = GetCustomerId();
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        public IActionResult Edit([Bind("Name,Email,Phone")] Models.Customer updated)
        {
            var customerId = GetCustomerId();
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(customer); // Giữ dữ liệu cũ để tránh mất thông tin
            }

            // Chỉ cập nhật những trường cho phép
            customer.Name = updated.Name;
            customer.Email = updated.Email;
            customer.Phone = updated.Phone;

            _context.SaveChanges();

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Index");
        }
    }
}
