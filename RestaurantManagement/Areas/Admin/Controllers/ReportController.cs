using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;
using System.IO;
using RestaurantManagement.Models;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            ViewBag.TotalRevenueToday = _context.Payments
                .Where(p => p.PaymentTime.Date == today)
                .Sum(p => (decimal?)p.TotalAmount) ?? 0;

            ViewBag.TotalRevenueMonth = _context.Payments
                .Where(p => p.PaymentTime >= thisMonth)
                .Sum(p => (decimal?)p.TotalAmount) ?? 0;

            ViewBag.TotalOrdersToday = _context.Orders
                .Count(o => o.OrderTime.Date == today);

            ViewBag.TopMenuItem = _context.OrderItems
                .Include(oi => oi.MenuItem)
                .GroupBy(oi => oi.MenuItem!.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(g => g.Quantity)
                .FirstOrDefault();

            ViewBag.FeedbackAvg = _context.Feedbacks.Any()
                ? _context.Feedbacks.Average(f => f.Rating)
                : 0;

            return View();
        }

        public IActionResult CanceledOrders()
        {
            var canceledOrders = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Where(o => o.Status == OrderStatus.Canceled)
                .OrderByDescending(o => o.OrderTime)
                .ToList();

            return View(canceledOrders);
        }

        public IActionResult RemovedOrderItems()
        {
            var data = _context.OrderItemHistories
                .OrderByDescending(h => h.RemovedAt)
                .ToList();

            return View(data);
        }

        public IActionResult RevenueReport(DateTime? from, DateTime? to)
        {
            var start = from ?? DateTime.Today.AddDays(-7);
            var end = to ?? DateTime.Today;

            // Truy vấn DB trước rồi xử lý nhóm sau
            var rawPayments = _context.Payments
                .Where(p => p.PaymentTime >= start && p.PaymentTime <= end)
                .ToList();

            var grouped = rawPayments
                .GroupBy(p => p.PaymentTime.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Amount = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            ViewBag.Labels = grouped.Select(x => x.Date).ToList();
            ViewBag.Data = grouped.Select(x => x.Amount).ToList();
            ViewBag.Total = grouped.Sum(x => x.Amount);
            ViewBag.From = start.ToString("yyyy-MM-dd");
            ViewBag.To = end.ToString("yyyy-MM-dd");

            return View();
        }

        public IActionResult ExportRevenue(DateTime from, DateTime to)
        {
            var rawPayments = _context.Payments
                .Where(p => p.PaymentTime >= from && p.PaymentTime <= to)
                .ToList();

            var grouped = rawPayments
                .GroupBy(p => p.PaymentTime.Date)
                .Select(g => new
                {
                    Ngay = g.Key.ToString("yyyy-MM-dd"),
                    TongTien = g.Sum(x => x.TotalAmount)
                }).ToList();

            using var ms = new MemoryStream();
            using (var writer = new StreamWriter(ms, leaveOpen: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(grouped);
            }

            ms.Position = 0;
            return File(ms.ToArray(), "text/csv", $"DoanhThu_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");
        }


        // 2. Báo cáo số lượng đơn theo trạng thái
        public async Task<IActionResult> OrderReport()
        {
            var data = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.Data = data;
            return View();
        }

        // 3. Báo cáo món ăn bán chạy nhất
        public async Task<IActionResult> MenuItemReport()
        {
            var data = await _context.OrderItems
                .Include(oi => oi.MenuItem)
                .GroupBy(oi => oi.MenuItem.Name)
                .Select(g => new
                {
                    MenuItem = g.Key,
                    Quantity = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(g => g.Quantity)
                .Take(10)
                .ToListAsync();

            ViewBag.Data = data;
            return View();
        }

        // 4. Báo cáo nhân viên phục vụ nhiều đơn nhất
        public async Task<IActionResult> StaffReport()
        {
            var data = await _context.Orders
                .Include(o => o.Staff)
                .GroupBy(o => o.Staff.Name)
                .Select(g => new
                {
                    StaffName = g.Key,
                    TotalOrders = g.Count()
                })
                .OrderByDescending(g => g.TotalOrders)
                .ToListAsync();

            ViewBag.Data = data;
            return View();
        }

        // 5. Báo cáo đánh giá khách hàng
        public async Task<IActionResult> FeedbackReport()
        {
            var data = await _context.Feedbacks
                .GroupBy(f => f.Rating)
                .Select(g => new
                {
                    Rating = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Rating)
                .ToListAsync();

            ViewBag.Data = data;
            return View();
        }

        // 6. Báo cáo nguyên liệu sử dụng nhiều nhất
        public async Task<IActionResult> InventoryReport()
        {
            var data = await _context.InventoryUsages
                .Include(i => i.InventoryItem)
                .GroupBy(i => i.InventoryItem.Name)
                .Select(g => new
                {
                    Item = g.Key,
                    TotalUsed = g.Sum(i => i.QuantityUsed)
                })
                .OrderByDescending(g => g.TotalUsed)
                .ToListAsync();

            ViewBag.Data = data;
            return View();
        }
    }
}
