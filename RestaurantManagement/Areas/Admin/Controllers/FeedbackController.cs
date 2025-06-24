using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestaurantManagement.Data;
using RestaurantManagement.Models;

namespace RestaurantManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }


        public IActionResult Index(int? rating, string? status, DateTime? from, DateTime? to)
        {
            var feedbacks = _context.Feedbacks
                .Include(f => f.Order).ThenInclude(o => o.Customer)
                .Include(f => f.Reply)
                .AsQueryable();

            // Mặc định lọc ngày hôm nay
            DateTime start = from ?? DateTime.Today;
            DateTime end = to?.Date.AddDays(1).AddTicks(-1) ?? DateTime.Today.AddDays(1).AddTicks(-1);

            feedbacks = feedbacks.Where(f => f.FeedbackTime >= start && f.FeedbackTime <= end);

            if (rating.HasValue)
                feedbacks = feedbacks.Where(f => f.Rating == rating.Value);

            if (status == "Replied")
                feedbacks = feedbacks.Where(f => f.Reply != null);
            else if (status == "Pending")
                feedbacks = feedbacks.Where(f => f.Reply == null);

            var list = feedbacks
                .OrderByDescending(f => f.FeedbackTime)
                .ToList();

            ViewBag.Rating = rating;
            ViewBag.Status = status;
            ViewBag.From = start.ToString("yyyy-MM-dd");
            ViewBag.To = end.ToString("yyyy-MM-dd");

            return View(list);
        }



        public IActionResult Details(int id)
        {
            var feedback = _context.Feedbacks
                .Include(f => f.Order)
                    .ThenInclude(o => o.Customer)
                .Include(f => f.Reply)
                .FirstOrDefault(f => f.FeedbackId == id);

            if (feedback == null) return NotFound();
            return View(feedback);
        }

        public IActionResult Reply(int id)
        {
            var feedback = _context.Feedbacks
                .Include(f => f.Order)
                    .ThenInclude(o => o.Customer)
                .Include(f => f.Reply)
                .FirstOrDefault(f => f.FeedbackId == id);

            if (feedback == null || feedback.Reply != null)
            {
                TempData["Error"] = "Phản hồi không tồn tại hoặc đã được trả lời.";
                return RedirectToAction("Index");
            }

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
                .Include(f => f.Reply)
                .FirstOrDefault(f => f.FeedbackId == feedbackId);

            if (feedback == null || feedback.Reply != null)
            {
                TempData["Error"] = "Phản hồi không hợp lệ hoặc đã được trả lời.";
                return RedirectToAction("Index");
            }

            var reply = new Reply
            {
                FeedbackId = feedbackId,
                Content = content,
                RepliedAt = DateTime.Now
            };

            _context.Replies.Add(reply);
            _context.SaveChanges();

            TempData["Success"] = "Đã phản hồi thành công!";
            return RedirectToAction("Details", new { id = feedbackId });
        }

        public IActionResult Delete(int id)
        {
            var feedback = _context.Feedbacks
                .Include(f => f.Reply)
                .FirstOrDefault(f => f.FeedbackId == id);

            if (feedback == null)
            {
                TempData["Error"] = "Không tìm thấy phản hồi.";
                return RedirectToAction("Index");
            }

            if (feedback.Reply != null)
                _context.Replies.Remove(feedback.Reply);

            _context.Feedbacks.Remove(feedback);
            _context.SaveChanges();

            TempData["Success"] = "Đã xoá phản hồi.";
            return RedirectToAction("Index");
        }
    }
}
