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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetCustomerId()
        {
            var customerIdStr = User.FindFirst("CustomerId")?.Value;
            return int.TryParse(customerIdStr, out int id) ? id : null;
        }

        public IActionResult Index()
        {
            var customerId = GetCustomerId();
            if (customerId == null) return Unauthorized();

            var customer = _context.Customers
                .Include(c => c.Orders).ThenInclude(o => o.Feedback)
                .Include(c => c.Reservations)
                .FirstOrDefault(c => c.CustomerId == customerId);

            if (customer == null) return NotFound();

            var recentOrders = customer.Orders?
                .OrderByDescending(o => o.OrderTime)
                .Take(5)
                .ToList();

            var recentReservations = customer.Reservations?
                .OrderByDescending(r => r.ReservationTime)
                .Take(5)
                .ToList();

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
