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
        private readonly IWebHostEnvironment _env;

        public FeedbackController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int? GetCustomerIdFromClaims()
        {
            var customerIdStr = User.FindFirst("CustomerId")?.Value;
            return int.TryParse(customerIdStr, out var id) ? id : null;
        }

        [HttpGet]
        public IActionResult Create(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        [HttpPost]
        public IActionResult Create(int orderId, string content, int rating, IFormFile? imageFile)
        {
            var customerId = GetCustomerIdFromClaims();
            if (customerId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(content) || rating < 1 || rating > 5)
            {
                ModelState.AddModelError("", "Vui lòng nhập nội dung và đánh giá hợp lệ.");
                ViewBag.OrderId = orderId;
                return View();
            }

            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId && o.CustomerId == customerId);
            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại hoặc không thuộc về bạn.";
                return RedirectToAction("Index", "Order");
            }

            var feedback = new Feedback
            {
                OrderId = orderId,
                Content = content,
                Rating = rating
            };

            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();

            // Xử lý ảnh nếu có
            if (imageFile != null && imageFile.Length > 0)
            {
                var ext = Path.GetExtension(imageFile.FileName);
                var fileName = $"{feedback.FeedbackId}{ext}";
                var path = Path.Combine(_env.WebRootPath, "images", "Feedback", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                feedback.ImagePath = $"/images/Feedback/{fileName}";
                _context.SaveChanges();
            }

            TempData["Success"] = "Gửi phản hồi thành công!";
            return RedirectToAction("Details", "Order", new { id = orderId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var customerId = GetCustomerIdFromClaims();
            if (customerId == null) return Unauthorized();

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
        public IActionResult Edit(int id, string content, int rating, IFormFile? imageFile)
        {
            var customerId = GetCustomerIdFromClaims();
            if (customerId == null) return Unauthorized();

            var feedback = _context.Feedbacks
                .Include(f => f.Order)
                .FirstOrDefault(f => f.FeedbackId == id && f.Order.CustomerId == customerId);

            if (feedback == null)
            {
                return RedirectToAction("Index", "Order");
            }

            if (string.IsNullOrWhiteSpace(content) || rating < 1 || rating > 5)
            {
                ModelState.AddModelError("", "Vui lòng nhập nội dung và đánh giá hợp lệ.");
                return View(feedback);
            }

            feedback.Content = content;
            feedback.Rating = rating;

            if (imageFile != null && imageFile.Length > 0)
            {
                var ext = Path.GetExtension(imageFile.FileName);
                var fileName = $"{feedback.FeedbackId}{ext}";
                var path = Path.Combine(_env.WebRootPath, "images", "Feedback", fileName);

                // Ghi đè ảnh cũ
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                feedback.ImagePath = $"/images/Feedback/{fileName}";
            }

            _context.SaveChanges();

            TempData["Success"] = "Cập nhật phản hồi thành công!";
            return RedirectToAction("Details", "Order", new { id = feedback.OrderId });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var customerId = GetCustomerIdFromClaims();
            if (customerId == null) return Unauthorized();

            var feedback = _context.Feedbacks
                .Include(f => f.Order)
                .FirstOrDefault(f => f.FeedbackId == id && f.Order.CustomerId == customerId);

            if (feedback == null)
            {
                return RedirectToAction("Index", "Order");
            }

            // Xóa ảnh nếu có
            if (!string.IsNullOrEmpty(feedback.ImagePath))
            {
                var filePath = Path.Combine(_env.WebRootPath, feedback.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Feedbacks.Remove(feedback);
            _context.SaveChanges();

            TempData["Success"] = "Đã xoá phản hồi.";
            return RedirectToAction("Details", "Order", new { id = feedback.OrderId });
        }

        [HttpGet]
        public IActionResult Index()
        {
            var customerId = GetCustomerIdFromClaims();
            if (customerId == null) return Unauthorized();

            var feedbacks = _context.Feedbacks
                .Include(f => f.Reply)
                .Include(f => f.Order)
                .Where(f => f.Order.CustomerId == customerId)
                .OrderByDescending(f => f.FeedbackId)
                .ToList();

            return View(feedbacks);
        }
    }
}
