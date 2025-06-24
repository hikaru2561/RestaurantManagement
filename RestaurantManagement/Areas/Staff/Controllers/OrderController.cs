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
                .Include(o => o.DingningTable)
                .Include(o => o.Customer)
                .Where(o => o.StaffId == staffId)
                .OrderByDescending(o => o.OrderTime)
                .ToList();
            return View(orders);
        }

        public IActionResult Add()
        {
            ViewBag.DingningTables = _context.DingningTables.Where(t => t.Status == TableStatus.Available).ToList();
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
                TempData["Error"] = "Vui lòng chọn ít nhất một món ăn.";
                return RedirectToAction(nameof(Add));
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            for (int i = 0; i < MenuItemIds.Length; i++)
            {
                if (Quantities[i] > 0)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        MenuItemId = MenuItemIds[i],
                        Quantity = Quantities[i]
                    };
                    _context.OrderItems.Add(orderItem);
                }
            }

            var table = _context.DingningTables.FirstOrDefault(t => t.DingningTableId == order.DingningTableId);
            if (table != null)
            {
                table.Status = TableStatus.InUse;
            }

            _context.SaveChanges();
            TempData["Success"] = "Tạo đơn hàng thành công.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            int staffId = GetStaffId();
            var order = _context.Orders
                .Include(o => o.DingningTable)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.OrderId == id && o.StaffId == staffId);

            if (order == null) return NotFound();

            ViewBag.MenuItems = _context.MenuItems.Where(m => m.Status).ToList();
            return View(order);
        }

        [HttpPost]
        public IActionResult AddItem(int orderId, int menuItemId, int quantity)
        {
            if (quantity <= 0)
            {
                TempData["Error"] = "Số lượng phải lớn hơn 0.";
                return RedirectToAction("Details", new { id = orderId });
            }

            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.OrderId == orderId && o.StaffId == GetStaffId());

            if (order == null) return NotFound();

            var existingItem = order.OrderItems.FirstOrDefault(i => i.MenuItemId == menuItemId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = orderId,
                    MenuItemId = menuItemId,
                    Quantity = quantity
                });
            }

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
            var staffId = GetStaffId();
            if (staffId == null || quantity < 1)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var orderItem = _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.MenuItem)
                .FirstOrDefault(oi => oi.OrderItemId == orderItemId && oi.Order.StaffId == staffId);

            if (orderItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy món ăn trong đơn." });
            }

            orderItem.Quantity = quantity;
            _context.SaveChanges();

            var orderId = orderItem.OrderId;
            var total = _context.OrderItems
                .Where(i => i.OrderId == orderId)
                .Sum(i => i.Quantity * i.MenuItem.Price);

            return Json(new
            {
                success = true,
                itemTotal = (orderItem.MenuItem.Price * quantity).ToString("N0"),
                totalAmount = total.ToString("N0")
            });
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == id && o.StaffId == GetStaffId());
            if (order == null || order.Status == OrderStatus.Paid)
            {
                TempData["Error"] = "Không thể hủy đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            order.Status = OrderStatus.Canceled;

            var table = _context.DingningTables.FirstOrDefault(t => t.DingningTableId == order.DingningTableId);
            if (table != null && table.Status == TableStatus.InUse)
            {
                table.Status = TableStatus.Available;
            }

            _context.SaveChanges();
            TempData["Success"] = "Đã hủy đơn hàng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Checkout(int id)
        {
            var staffId = GetStaffId();
            var order = _context.Orders
                .Include(o => o.OrderItems).ThenInclude(i => i.MenuItem)
                .Include(o => o.DingningTable)
                .Include(o => o.Customer)
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.OrderId == id && o.StaffId == staffId);

            if (order == null || (order.Status != OrderStatus.Paid && order.Status != OrderStatus.Completed))
            {
                TempData["Error"] = "Đơn hàng không tồn tại hoặc chưa thanh toán.";
                return RedirectToAction("Index");
            }

            return View("Checkout", order);
        }


        [HttpPost]
        public IActionResult Checkout(int id, string method)
        {
            var staffId = GetStaffId();
            if (staffId == null)
            {
                TempData["Error"] = "Không xác định được nhân viên.";
                return RedirectToAction("Index");
            }

            var order = _context.Orders
                .Include(o => o.OrderItems).ThenInclude(i => i.MenuItem)
                .Include(o => o.DingningTable)
                .Include(o => o.Customer)
                .FirstOrDefault(o => o.OrderId == id && o.StaffId == staffId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Details", new { id });
            }

            if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Completed)
            {
                TempData["Error"] = "Đơn hàng đã được thanh toán hoặc hoàn tất.";
                return RedirectToAction("Details", new { id });
            }

            decimal total = order.OrderItems.Sum(i => i.Quantity * i.MenuItem.Price);

            // Tạo bản ghi thanh toán
            var payment = new Payment
            {
                OrderId = order.OrderId,
                PaymentTime = DateTime.Now,
                TotalAmount = total,
                Method = method
            };

            _context.Payments.Add(payment);

            // Cập nhật trạng thái đơn hàng
            order.Status = OrderStatus.Completed;

            // Cập nhật trạng thái bàn
            order.DingningTable.Status = TableStatus.Available;

            _context.SaveChanges();
            TempData["Success"] = "Thanh toán thành công.";

            return RedirectToAction("Details", new { id });
        }



    }
}
