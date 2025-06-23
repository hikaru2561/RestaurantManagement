using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using System;
using System.Linq;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int orderItemId)
        {
            var orderItem = await _context.OrderItems
                .Include(o => o.MenuItem)
                .FirstOrDefaultAsync(o => o.OrderItemId == orderItemId);

            if (orderItem == null)
            {
                TempData["Error"] = "Không tìm thấy món cần xóa!";
                return RedirectToAction("Index");
            }

            // Ghi log vào lịch sử
            var log = new OrderItemHistory
            {
                OrderId = orderItem.OrderId,
                MenuItemId = orderItem.MenuItemId,
                MenuItemName = orderItem.MenuItem?.Name,
                Quantity = orderItem.Quantity,
                RemovedAt = DateTime.Now,
                RemovedBy = HttpContext.Session.GetString("Username") ?? "Unknown"
            };

            _context.OrderItemHistories.Add(log);

            // Xóa món
            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa món khỏi đơn hàng và lưu vào lịch sử!";
            return RedirectToAction("Details", new { id = orderItem.OrderId });
        }

        public IActionResult Index(string searchTable, OrderStatus? statusFilter, DateTime? dateFilter)
        {
            var orders = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Staff)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTable))
                orders = orders.Where(o => o.Table.Name.Contains(searchTable));

            if (statusFilter.HasValue)
                orders = orders.Where(o => o.Status == statusFilter);

            if (dateFilter.HasValue)
                orders = orders.Where(o => o.OrderTime.Date == dateFilter.Value.Date);


            var orderList = orders.OrderByDescending(o => o.OrderTime).ToList();
            return View(orderList);
        }

        public IActionResult Create()
        {
            ViewBag.Tables = _context.DingningTables.ToList();
            ViewBag.Staffs = _context.Staffs.ToList();
            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.MenuItems = _context.MenuItems.Where(m => m.Status).ToList();
            return View(new Order { OrderTime = DateTime.Now });
        }

        [HttpPost]
        public IActionResult Create(Order order, int[] MenuItemIds, int[] Quantities)
        {
            if (ModelState.IsValid)
            {
                order.OrderTime = DateTime.Now;
                order.Status = OrderStatus.Ordered;

                _context.Orders.Add(order);
                _context.SaveChanges();

                for (int i = 0; i < MenuItemIds.Length; i++)
                {
                    if (Quantities[i] > 0)
                    {
                        var item = new OrderItem
                        {
                            OrderId = order.OrderId,
                            MenuItemId = MenuItemIds[i],
                            Quantity = Quantities[i]
                        };
                        _context.OrderItems.Add(item);
                    }
                }

                _context.SaveChanges();
                TempData["Success"] = "Tạo đơn hàng thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Tạo đơn hàng thất bại!";
            return View(order);
        }

        public IActionResult Edit(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null) return NotFound();

            ViewBag.Tables = _context.DingningTables.ToList();
            ViewBag.MenuItems = _context.MenuItems.ToList();
            ViewBag.Staffs = _context.Staffs.ToList();
            ViewBag.Customers = _context.Customers.ToList();

            return View(order);
        }

        [HttpPost]
        public IActionResult Edit(Order model)
        {
            var order = _context.Orders.Find(model.OrderId);
            if (order == null) return NotFound();

            order.Status = model.Status;
            _context.SaveChanges();

            TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null) return NotFound();
            return View(order);
        }

        public IActionResult Delete(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.OrderId == id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.OrderId == id);

            if (order != null)
            {
                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
                _context.SaveChanges();
                TempData["Success"] = "Xoá đơn hàng thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
