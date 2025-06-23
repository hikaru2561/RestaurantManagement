using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagement.Models
{
    public class InventoryTransaction
    {
        public int InventoryTransactionId { get; set; }

        [Required]
        public int InventoryItemId { get; set; }

        [Required]
        public float Quantity { get; set; }

        [Required]
        public string Type { get; set; } // Nhập / Xuất / Huỷ

        [Required]
        public DateTime CreatedAt { get; set; }

        // Navigation
        [ForeignKey("InventoryItemId")]
        public InventoryItem? InventoryItem { get; set; }
    }
}
