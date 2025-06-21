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

        private string GetUsername() => User.FindFirst(ClaimTypes.Name)?.Value ?? "";

        public IActionResult Index()
        {
            var username = GetUsername();
            var customer = _context.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpGet]
        public IActionResult Edit()
        {
            var username = GetUsername();
            var customer = _context.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        public IActionResult Edit(Models.Customer updated)
        {
            var username = GetUsername();
            var customer = _context.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return NotFound();
            }

            customer.Name = updated.Name;
            customer.Email = updated.Email;
            customer.Phone = updated.Phone;
            _context.SaveChanges();

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Index");
        }
    }
}
