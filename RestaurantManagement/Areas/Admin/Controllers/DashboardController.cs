using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using RestaurantManagement.Models.ViewModels;
using System;
using System.Linq;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.User = User.Identity?.Name;
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var model = new DashboardViewModel
            {
                TodayOrderCount = _context.Orders.Count(o => o.OrderTime.Date == today),
                TodayRevenue = _context.Orders
                    .Where(o => o.OrderTime.Date == today && o.Status == OrderStatus.Completed && o.Payment != null)
                    .Sum(o => o.Payment.TotalAmount),

                MonthRevenue = _context.Orders
                    .Where(o => o.OrderTime >= startOfMonth && o.Status == OrderStatus.Completed && o.Payment != null)
                    .Sum(o => o.Payment.TotalAmount),

                TodayReservationCount = _context.Reservations.Count(r => r.ReservationTime.Date == today),
                AverageRating = _context.Feedbacks.Any() ? _context.Feedbacks.Average(f => f.Rating) : 0,

                TopMenuItems = _context.OrderItems
                    .Include(oi => oi.MenuItem)
                    .GroupBy(oi => oi.MenuItem!.Name)
                    .Select(g => new MenuItemStat
                    {
                        Name = g.Key,
                        TotalSold = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(g => g.TotalSold)
                    .Take(5)
                    .ToList(),

                WeeklyRevenue = Enumerable.Range(0, 7)
                    .Select(i => today.AddDays(-i))
                    .Select(date => new DailyRevenue
                    {
                        Date = date,
                        Revenue = _context.Orders
                            .Where(o => o.OrderTime.Date == date && o.Status == OrderStatus.Completed && o.Payment != null)
                            .Sum(o => o.Payment.TotalAmount)
                    })
                    .OrderBy(r => r.Date)
                    .ToList()
            };

            return View(model);
        }
    }

}