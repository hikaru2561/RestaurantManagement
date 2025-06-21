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
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCustomerId()
        {
            var username = User.Identity.Name;
            return _context.Customers.FirstOrDefault(c => c.Username == username)?.CustomerId ?? 0;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var customerId = GetCustomerId();
            var feedbacks = _context.Feedbacks
                .Include(f => f.Reply)
                .Include(f => f.Order)
                .Where(f => f.Order.CustomerId == customerId)  // Sửa thành kiểm tra qua Order.CustomerId
                .OrderByDescending(f => f.FeedbackId)
                .ToList();

            return View(feedbacks);
        }

        [HttpGet]
        public IActionResult Create(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        [HttpPost]
        public IActionResult Create(int orderId, string content, int rating)
        {
            var customerId = GetCustomerId();

            // Kiểm tra order thuộc khách
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId && o.CustomerId == customerId);
            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại hoặc không thuộc về bạn.";
                return RedirectToAction("Index", "Order");
            }

            if (string.IsNullOrWhiteSpace(content) || rating < 1 || rating > 5)
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ nội dung và đánh giá.");
                ViewBag.OrderId = orderId;
                return View();
            }

            var feedback = new Feedback
            {
                OrderId = orderId,
                Content = content,
                Rating = rating
            };

            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();

            TempData["Success"] = "Gửi phản hồi thành công!";
            return RedirectToAction("Details", "Order", new { id = orderId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var customerId = GetCustomerId();
            var feedback = _context.Feedbacks
                .Include(f => f.Reply)
                .Include(f => f.Order)
                .FirstOrDefault(f => f.FeedbackId == id && f.Order.CustomerId == customerId);

            if (feedback == null || feedback.Reply != null)
            {
                return RedirectToAction("Index", "Order");
            }

            return View(feedback);
        }

        [HttpPost]
        public IActionResult Edit(int id, string content, int rating)
        {
            var customerId = GetCustomerId();
            var feedback = _context.Feedbacks
                .Include(f => f.Reply)
                .Include(f => f.Order)
                .FirstOrDefault(f => f.FeedbackId == id && f.Order.CustomerId == customerId);

            if (feedback == null || feedback.Reply != null)
            {
                return RedirectToAction("Index", "Order");
            }

            if (string.IsNullOrWhiteSpace(content) || rating < 1 || rating > 5)
            {
                ModelState.AddModelError("", "Vui lòng nhập nội dung hợp lệ.");
                return View(feedback);
            }

            feedback.Content = content;
            feedback.Rating = rating;
            _context.SaveChanges();

            TempData["Success"] = "Cập nhật phản hồi thành công!";
            return RedirectToAction("Details", "Order", new { id = feedback.OrderId });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var customerId = GetCustomerId();
            var feedback = _context.Feedbacks
                .Include(f => f.Reply)
                .Include(f => f.Order)
                .FirstOrDefault(f => f.FeedbackId == id && f.Order.CustomerId == customerId);

            if (feedback == null || feedback.Reply != null)
            {
                return RedirectToAction("Index", "Order");
            }

            _context.Feedbacks.Remove(feedback);
            _context.SaveChanges();

            TempData["Success"] = "Đã xoá phản hồi.";
            return RedirectToAction("Details", "Order", new { id = feedback.OrderId });
        }
    }
}
