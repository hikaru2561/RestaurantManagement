using RestaurantManagement.Data;
using RestaurantManagement.Models;

namespace RestaurantManagement.Helpers
{
    public static class NotificationHelper
    {
        public static void AddNotification(ApplicationDbContext context, string title, string? content, string recipientUsername, string role)
        {
            var notification = new Notification
            {
                Title = title,
                Content = content,
                CreatedAt = DateTime.Now,
                RecipientUsername = recipientUsername,
                Role = role,
                IsRead = false
            };

            context.Notifications.Add(notification);
            context.SaveChanges();
        }
    }
}
