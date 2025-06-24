using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using System.Security.Claims;

namespace RestaurantManagement.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class ExpenseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpenseController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetCustomerId()
        {
            var customerIdStr = User.FindFirst("CustomerId")?.Value;
            return int.TryParse(customerIdStr, out var id) ? id : null;
        }

        public async Task<IActionResult> Index()
        {
            var customerId = GetCustomerId();
            if (customerId == null)
                return Unauthorized();

            var orders = await _context.Orders
                .Include(o => o.Payment)
                .Include(o => o.DingningTable)
                .Where(o => o.CustomerId == customerId &&
                    (o.Status == OrderStatus.Paid || o.Status == OrderStatus.Completed))
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();

            return View(orders);
        }
    }
}
