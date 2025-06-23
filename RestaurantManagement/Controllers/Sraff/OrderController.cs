using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using System.Security.Claims;

namespace RestaurantManagement.Controllers.Staff
{


    [Authorize(Roles = "Staff")]
    [Route("Staff/Order")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewBag.Tables = _context.DingningTables.ToList();
            ViewBag.MenuItems = _context.MenuItems.Where(m => m.Status).ToList();
            ViewBag.Customers = _context.Customers.ToList();

            return View();
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(int TableId, int? CustomerId, List<int> MenuItemIds, List<int> Quantities)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Username == username);
            if (staff == null)
            {
                TempData["Error"] = "Không tìm thấy nhân viên.";
                return RedirectToAction("Create");
            }

            if (MenuItemIds == null || !MenuItemIds.Any() || Quantities == null || MenuItemIds.Count != Quantities.Count)
            {
                TempData["Error"] = "Vui lòng chọn món và nhập số lượng.";
                return RedirectToAction("Create");
            }

            var order = new Order
            {
                TableId = TableId,
                CustomerId = CustomerId,
                StaffId = staff.StaffId,
                OrderTime = DateTime.Now,
                Status = OrderStatus.Ordered,
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            for (int i = 0; i < MenuItemIds.Count; i++)
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

            await _context.SaveChangesAsync();
            TempData["Success"] = "Tạo đơn hàng thành công!";
            return RedirectToAction("Index");
        }
    }
}
