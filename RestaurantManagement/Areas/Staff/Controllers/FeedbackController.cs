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

        private int? GetStaffIdFromClaims()
        {
            var staffIdStr = User.FindFirst("StaffId")?.Value;
            return int.TryParse(staffIdStr, out var id) ? id : null;
        }

        public IActionResult Index()
        {
            var staffId = GetStaffIdFromClaims();
            if (staffId == null) return Unauthorized();

            var feedbacks = _context.Feedbacks
                .Include(f => f.Order)
                .Include(f => f.Reply)
                .Where(f => f.Order != null && f.Order.StaffId == staffId)
                .ToList();

            return View(feedbacks);
        }

        public IActionResult Details(int id)
        {
            var staffId = GetStaffIdFromClaims();
            if (staffId == null) return Unauthorized();

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

            var staffId = GetStaffIdFromClaims();
            if (staffId == null) return Unauthorized();

            var feedback = _context.Feedbacks
                .Include(f => f.Order)
                .Include(f => f.Reply)
                .FirstOrDefault(f => f.FeedbackId == feedbackId && f.Order.StaffId == staffId);

            if (feedback == null)
            {
                TempData["Error"] = "Phản hồi không hợp lệ hoặc không thuộc quyền truy cập.";
                return RedirectToAction("Index");
            }

            if (feedback.Reply != null)
            {
                TempData["Error"] = "Phản hồi này đã được trả lời trước đó.";
                return RedirectToAction("Details", new { id = feedbackId });
            }

            var reply = new Reply
            {
                FeedbackId = feedbackId,
                Content = content,
                RepliedAt = DateTime.Now
            };

            _context.Replies.Add(reply);
            _context.SaveChanges();

            TempData["Success"] = "Phản hồi thành công!";
            return RedirectToAction("Details", new { id = feedbackId });
        }
    }
}
