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
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCustomerId()
        {
            var idStr = User.FindFirst("CustomerId")?.Value;
            return int.TryParse(idStr, out var id) ? id : 0;
        }

        public IActionResult Index(string statusFilter, DateTime? fromDate, DateTime? toDate)
        {
            int customerId = GetCustomerId();
            var orders = _context.Orders
                .Include(o => o.DingningTable)
                .Include(o => o.Payment)
                .Where(o => o.CustomerId == customerId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter) &&
                Enum.TryParse<OrderStatus>(statusFilter, out var status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            if (fromDate.HasValue)
                orders = orders.Where(o => o.OrderTime >= fromDate.Value);

            if (toDate.HasValue)
                orders = orders.Where(o => o.OrderTime <= toDate.Value);

            return View(orders.OrderByDescending(o => o.OrderTime).ToList());
        }

        public IActionResult Details(int id)
        {
            int customerId = GetCustomerId();
            var order = _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .Include(o => o.DingningTable)
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.OrderId == id && o.CustomerId == customerId);

            if (order == null) return NotFound();

            var feedback = _context.Feedbacks
                .Include(f => f.Reply)
                .FirstOrDefault(f => f.OrderId == id);

            ViewBag.Feedback = feedback;
            return View(order);
        }
    }
}
