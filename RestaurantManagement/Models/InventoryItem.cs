using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Models
{
    public class InventoryItem
    {
        public int InventoryItemId { get; set; }

        [Required]
        public string Name { get; set; }

        public int Quantity { get; set; }

        public string Unit { get; set; }

        public string? ImagePath { get; set; }

        // Các bảng liên quan còn giữ lại:
        public ICollection<InventoryTransaction>? Transactions { get; set; }
        public ICollection<InventoryUsage>? Usages { get; set; }
    }
}
