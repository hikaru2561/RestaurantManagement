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

        private int GetCustomerId()
        {
            var username = User.Identity?.Name;
            var customer = _context.Customers.FirstOrDefault(c => c.Username == username);
            return customer?.CustomerId ?? 0;
        }

        public async Task<IActionResult> Index()
        {
            var customerId = GetCustomerId();
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
