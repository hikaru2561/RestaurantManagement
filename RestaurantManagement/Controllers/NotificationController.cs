using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using System.Linq;
using System.Security.Claims;

namespace RestaurantManagement.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get(int id)
        {
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var noti = _context.Notifications.FirstOrDefault(n =>
                n.NotificationId == id &&
                n.RecipientUsername == username &&
                n.Role == role);

            if (noti == null) return NotFound();

            noti.IsRead = true;
            _context.SaveChanges();

            return Json(new
            {
                title = noti.Title,
                content = noti.Content,
                createdAt = noti.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                senderName = noti.SenderName
            });
        }


        [HttpPost]
        public IActionResult MarkAllAsRead()
        {
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var unread = _context.Notifications
                .Where(n => n.RecipientUsername == username && n.Role == role && !n.IsRead)
                .ToList();

            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var noti = _context.Notifications
                .FirstOrDefault(n => n.NotificationId == id && n.RecipientUsername == username && n.Role == role);

            if (noti == null) return NotFound();

            _context.Notifications.Remove(noti);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost]
        public IActionResult Reply([FromBody] Notification model)
        {
            var senderUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername)) return Unauthorized();

            var notification = new Notification
            {
                Title = model.Title,
                Content = model.Content,
                RecipientUsername = model.RecipientUsername,
                Role = model.Role,
                CreatedAt = DateTime.Now,
                IsRead = false,
                SenderName = senderUsername // đảm bảo Notification có thuộc tính này
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet]
        public IActionResult GetLatest()
        {
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
                return Unauthorized();

            var notifications = _context.Notifications
                .Where(n => n.RecipientUsername == username && n.Role == role)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .Select(n => new
                {
                    n.NotificationId,
                    n.Title,
                    CreatedAt = n.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    n.IsRead
                })
                .ToList();

            var unreadCount = notifications.Count(n => !n.IsRead);

            return Json(new { unreadCount, notifications });
        }


    }
}
