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
                .Include(f => f.Order).ThenInclude(o => o.Customer)
                .Include(f => f.Reply)
                .Where(f => f.Order != null && f.Order.StaffId == staffId)
                .OrderByDescending(f => f.FeedbackTime)
                .ToList();

            return View(feedbacks);
        }

        public IActionResult Details(int id)
        {
            var staffId = GetStaffIdFromClaims();
            if (staffId == null) return Unauthorized();

            var feedback = _context.Feedbacks
                .Include(f => f.Order).ThenInclude(o => o.Customer)
                .Include(f => f.Reply)
                .FirstOrDefault(f => f.FeedbackId == id && f.Order.StaffId == staffId);

            if (feedback == null)
            {
                TempData["Error"] = "Phản hồi không tồn tại hoặc không thuộc quyền truy cập.";
                return RedirectToAction("Index");
            }

            return View(feedback);
        }

        // Tất cả các hành động như Reply, Delete... đều đã bị loại bỏ theo yêu cầu
    }
}
