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
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
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

            var feedbacks = _context.Feedbacks
                // Bỏ .Include(f => f.Customer) vì Feedback không có Customer navigation nữa
                .Include(f => f.Order)
                .Include(f => f.Reply)
                .Where(f => f.Order != null && f.Order.StaffId == staffId)
                .ToList();

            return View(feedbacks);
        }

        public IActionResult Details(int id)
        {
            int staffId = GetStaffId();

            var feedback = _context.Feedbacks
                .Include(f => f.Order)
                .Include(f => f.Reply)
                .FirstOrDefault(f => f.FeedbackId == id && f.Order.StaffId == staffId);

            if (feedback == null) return NotFound();

            return View(feedback);
        }

        [HttpPost]
        public IActionResult Reply(int feedbackId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Nội dung phản hồi không được để trống.";
                return RedirectToAction("Details", new { id = feedbackId });
            }

            var feedback = _context.Feedbacks
                .Include(f => f.Order)
                .Include(f => f.Reply)
                .FirstOrDefault(f => f.FeedbackId == feedbackId);

            if (feedback == null || feedback.Reply != null)
            {
                TempData["Error"] = "Phản hồi không hợp lệ hoặc đã được trả lời.";
                return RedirectToAction("Index");
            }

            int staffId = GetStaffId();

            var reply = new Reply
            {
                FeedbackId = feedbackId,
                Content = content,
                RepliedAt = DateTime.Now
                // Bạn có thể thêm thông tin staffId hoặc staff nếu cần mở rộng
            };

            _context.Replies.Add(reply);
            _context.SaveChanges();

            TempData["Success"] = "Phản hồi thành công!";
            return RedirectToAction("Details", new { id = feedbackId });
        }
    }
}
