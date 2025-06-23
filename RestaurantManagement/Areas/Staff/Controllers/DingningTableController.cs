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
    public class DingningTableController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DingningTableController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var tables = _context.DingningTables.ToList();
            return View(tables);
        }

        private int GetStaffId()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            return _context.Staffs.FirstOrDefault(s => s.Username == username)?.StaffId ?? 0;
        }

        // CHUYỂN BÀN
        [HttpGet]
        public IActionResult Transfer(int fromTableId)
        {
            ViewBag.FromTableId = fromTableId;
            ViewBag.Tables = _context.DingningTables
                .Where(t => t.DingningTableId != fromTableId && t.Status == TableStatus.Available)
                .ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Transfer(int fromTableId, int toTableId)
        {
            var fromTable = _context.DingningTables.FirstOrDefault(t => t.DingningTableId == fromTableId);
            var toTable = _context.DingningTables.FirstOrDefault(t => t.DingningTableId == toTableId);

            if (fromTable == null || toTable == null || fromTable.Status != TableStatus.InUse || toTable.Status != TableStatus.Available)
            {
                TempData["Error"] = "Chuyển bàn không hợp lệ.";
                return RedirectToAction("Index");
            }

            var order = _context.Orders
                .FirstOrDefault(o => o.DingningTableId == fromTableId && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Canceled);

            if (order != null)
            {
                order.DingningTableId = toTableId;
                fromTable.Status = TableStatus.Available;
                toTable.Status = TableStatus.InUse;
                _context.SaveChanges();
                TempData["Success"] = "Chuyển bàn thành công.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy đơn hàng để chuyển.";
            }

            return RedirectToAction("Index");
        }

        // GỘP BÀN
        [HttpGet]
        public IActionResult Merge(int mainTableId)
        {
            ViewBag.MainTableId = mainTableId;
            ViewBag.Tables = _context.DingningTables
                .Where(t => t.DingningTableId != mainTableId && t.Status == TableStatus.InUse)
                .ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Merge(int mainTableId, int mergeTableId)
        {
            var mainOrder = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.DingningTableId == mainTableId && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Canceled);

            var mergeOrder = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.DingningTableId == mergeTableId && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Canceled);

            if (mainOrder == null || mergeOrder == null)
            {
                TempData["Error"] = "Không thể gộp bàn.";
                return RedirectToAction("Index");
            }

            foreach (var item in mergeOrder.OrderItems!)
            {
                item.OrderId = mainOrder.OrderId;
            }

            _context.Orders.Remove(mergeOrder);

            var mergeTable = _context.DingningTables.Find(mergeTableId);
            if (mergeTable != null) mergeTable.Status = TableStatus.Available;

            _context.SaveChanges();
            TempData["Success"] = "Gộp bàn thành công.";
            return RedirectToAction("Index");
        }


        // TÁCH BÀN
        [HttpGet]
        // GET: Staff/Table/Split/5
        public IActionResult Split(int fromTableId)
        {
            var fromTable = _context.DingningTables.Find(fromTableId);
            if (fromTable == null || fromTable.Status != TableStatus.InUse)
            {
                TempData["Error"] = "Bàn không hợp lệ hoặc không đang sử dụng.";
                return RedirectToAction("Index");
            }

            // Danh sách bàn trống để chọn tách sang
            var emptyTables = _context.DingningTables
                .Where(t => t.Status == TableStatus.Available && t.DingningTableId != fromTableId)
                .ToList();

            // Lấy Order đang hoạt động trên bàn
            var order = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefault(o => o.DingningTableId == fromTableId &&
                                    (o.Status == OrderStatus.Ordered || o.Status == OrderStatus.Preparing));

            ViewBag.Tables = emptyTables;
            ViewBag.FromTableId = fromTableId;
            ViewBag.OrderItems = order?.OrderItems?.ToList() ?? new List<OrderItem>();

            return View(fromTableId);
        }

        // POST: Staff/Table/Split
        [HttpPost]
        public IActionResult Split(int fromTableId, int toTableId, List<int> selectedOrderItemIds)
        {
            if (fromTableId == toTableId)
            {
                TempData["Error"] = "Không thể tách cùng một bàn.";
                return RedirectToAction("Split", new { fromTableId });
            }

            var fromOrder = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.DingningTableId == fromTableId &&
                                    (o.Status == OrderStatus.Ordered || o.Status == OrderStatus.Preparing));

            if (fromOrder == null || selectedOrderItemIds == null || !selectedOrderItemIds.Any())
            {
                TempData["Error"] = "Không có đơn hàng để tách hoặc không chọn món.";
                return RedirectToAction("Split", new { fromTableId });
            }

            // Lấy thông tin Staff hiện tại
            var staffId = GetStaffId();

            // Tạo Order mới cho bàn mới
            var newOrder = new Order
            {
                DingningTableId = toTableId,
                CustomerId = fromOrder.CustomerId,
                OrderTime = DateTime.Now,
                Status = OrderStatus.Ordered,
                StaffId = staffId
            };
            _context.Orders.Add(newOrder);
            _context.SaveChanges(); // để có OrderId mới

            // Di chuyển món được chọn sang Order mới
            foreach (var itemId in selectedOrderItemIds)
            {
                var item = fromOrder.OrderItems.FirstOrDefault(oi => oi.OrderItemId == itemId);
                if (item != null)
                {
                    item.OrderId = newOrder.OrderId;
                }
            }

            // Cập nhật trạng thái bàn mới
            var toTable = _context.DingningTables.Find(toTableId);
            if (toTable != null)
            {
                toTable.Status = TableStatus.InUse;
            }

            _context.SaveChanges();
            TempData["Success"] = "Tách món sang bàn mới thành công!";
            return RedirectToAction("Index");
        }

    }
}
