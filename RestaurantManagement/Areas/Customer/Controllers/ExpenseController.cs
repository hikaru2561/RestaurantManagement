using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

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

        private int? GetCustomerIdFromClaims()
        {
            var customerIdStr = User.FindFirst("CustomerId")?.Value;
            return int.TryParse(customerIdStr, out var id) ? id : null;
        }

        public async Task<IActionResult> Index()
        {
            var customerId = GetCustomerIdFromClaims();
            if (customerId == null) return Unauthorized();

            var orders = await _context.Orders
                .Include(o => o.Payment)
                .Include(o => o.Table)
                .Where(o => o.CustomerId == customerId && o.Status == Models.OrderStatus.Paid)
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();

            return View(orders);
        }
    }
}
