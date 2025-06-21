using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Data;
using System.Linq;
using System.Security.Claims;

namespace RestaurantManagement.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
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
    }
}
