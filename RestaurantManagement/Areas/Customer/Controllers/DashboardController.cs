using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using System.Security.Claims;
using System.Linq;

namespace RestaurantManagement.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUsername() => User.FindFirst(ClaimTypes.Name)?.Value ?? "";

        public IActionResult Index()
        {
            var username = GetUsername();
            var customer = _context.Customers
                .Include(c => c.Orders)
                    .ThenInclude(o => o.Feedback)  // Load Feedback qua Order
                .Include(c => c.Reservations)
                .FirstOrDefault(c => c.Username == username);

            if (customer == null) return NotFound();

            var recentOrders = customer.Orders?
                .OrderByDescending(o => o.OrderTime)
                .Take(5)
                .ToList();

            var recentReservations = customer.Reservations?
                .OrderByDescending(r => r.ReservationTime)
                .Take(5)
                .ToList();

            // Lấy Feedback cuối cùng qua Orders
            var lastFeedback = customer.Orders?
                .Where(o => o.Feedback != null)
                .OrderByDescending(o => o.OrderTime)
                .Select(o => o.Feedback)
                .FirstOrDefault();

            ViewBag.Customer = customer;
            ViewBag.RecentOrders = recentOrders;
            ViewBag.RecentReservations = recentReservations;
            ViewBag.LastFeedback = lastFeedback;

            return View();
        }
    }
}
