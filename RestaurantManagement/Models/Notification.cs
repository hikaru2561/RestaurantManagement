using Microsoft.AspNetCore.Mvc;


namespace RestaurantManagement.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }

        public string Title { get; set; }

        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public string RecipientUsername { get; set; } // Ai nhận thông báo

        public string? SenderUsername { get; set; }
        public string? SenderName { get; set; } // Tên người gửi hiển thị
        
        public string Role { get; set; } // "Admin", "Staff", "Customer"
    }
}



