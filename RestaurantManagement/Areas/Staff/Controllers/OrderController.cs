using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using System.Security.Claims;

namespace RestaurantManagement.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetStaffId()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            return _context.Staffs.FirstOrDefault(s => s.Username == username)?.StaffId ?? 0;
        }

        public IActionResult Index()
        {
            int staffId = GetStaffId();
            var orders = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Customer)
                .Where(o => o.StaffId == staffId)
                .OrderByDescending(o => o.OrderTime)
                .ToList();
            return View(orders);
        }

        public IActionResult Add()
        {
            ViewBag.Tables = _context.Tables.Where(t => t.Status == TableStatus.Available).ToList();
            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.MenuItems = _context.MenuItems.Where(m => m.Status).ToList();
            return View(new Order { OrderTime = DateTime.Now });
        }

        [HttpPost]
        public IActionResult Add(Order order, int[] MenuItemIds, int[] Quantities)
        {
            int staffId = GetStaffId();
            order.StaffId = staffId;
            order.OrderTime = DateTime.Now;
            order.Status = OrderStatus.Ordered;

            if (MenuItemIds.Length != Quantities.Length || MenuItemIds.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất 1 món ăn.";
                return View(order);
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            for (int i = 0; i < MenuItemIds.Length; i++)
            {
                if (Quantities[i] > 0)
                {
                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        MenuItemId = MenuItemIds[i],
                        Quantity = Quantities[i]
                    });
                }
            }

            var table = _context.Tables.FirstOrDefault(t => t.TableId == order.TableId);
            if (table != null) table.Status = TableStatus.InUse;

            _context.SaveChanges();
            TempData["Success"] = "Tạo đơn hàng thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            int staffId = GetStaffId();
            var order = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.OrderId == id && o.StaffId == staffId);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        public IActionResult AddItem(int orderId, int menuItemId, int quantity)
        {
            var order = _context.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return NotFound();

            var existing = order.OrderItems.FirstOrDefault(i => i.MenuItemId == menuItemId);
            if (existing != null)
                existing.Quantity += quantity;
            else
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = orderId,
                    MenuItemId = menuItemId,
                    Quantity = quantity
                });

            _context.SaveChanges();
            TempData["Success"] = "Thêm món thành công.";
            return RedirectToAction("Details", new { id = orderId });
        }

        [HttpPost]
        public IActionResult RemoveItem(int orderItemId)
        {
            var item = _context.OrderItems.FirstOrDefault(o => o.OrderItemId == orderItemId);
            if (item == null) return NotFound();

            int orderId = item.OrderId;
            _context.OrderItems.Remove(item);
            _context.SaveChanges();

            TempData["Success"] = "Đã xóa món khỏi đơn.";
            return RedirectToAction("Details", new { id = orderId });
        }

        [HttpPost]
        public IActionResult UpdateItemQuantity(int orderItemId, int quantity)
        {
            var item = _context.OrderItems.FirstOrDefault(o => o.OrderItemId == orderItemId);
            if (item == null) return NotFound();

            item.Quantity = quantity;
            _context.SaveChanges();

            TempData["Success"] = "Cập nhật số lượng thành công.";
            return RedirectToAction("Details", new { id = item.OrderId });
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == id && o.StaffId == GetStaffId());
            if (order == null || order.Status == OrderStatus.Paid)
            {
                TempData["Error"] = "Không thể hủy đơn hàng.";
                return RedirectToAction("Index");
            }

            order.Status = OrderStatus.Canceled;
            _context.SaveChanges();

            TempData["Success"] = "Đã hủy đơn hàng.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Checkout(int id, string method)
        {
            var order = _context.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.OrderId == id && o.StaffId == GetStaffId());
            if (order == null || order.Status == OrderStatus.Paid)
            {
                TempData["Error"] = "Đơn hàng không hợp lệ.";
                return RedirectToAction("Details", new { id });
            }

            decimal total = order.OrderItems.Sum(i => i.Quantity * i.MenuItem.Price);

            _context.Payments.Add(new Payment
            {
                OrderId = order.OrderId,
                PaymentTime = DateTime.Now,
                TotalAmount = total,
                Method = method
            });

            order.Status = OrderStatus.Paid;
            _context.SaveChanges();

            TempData["Success"] = "Thanh toán thành công.";
            return RedirectToAction("Details", new { id });
        }
    }
}
