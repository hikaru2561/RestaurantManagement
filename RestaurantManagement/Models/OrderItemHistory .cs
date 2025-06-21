using Microsoft.AspNetCore.Mvc;

namespace RestaurantManagement.Models
{
    public class OrderItemHistory
    {
        public int OrderItemHistoryId { get; set; }

        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public string? MenuItemName { get; set; } // Lưu tên món lúc bị xóa
        public int Quantity { get; set; }
        public DateTime RemovedAt { get; set; }

        public string? RemovedBy { get; set; } 
    }

}
