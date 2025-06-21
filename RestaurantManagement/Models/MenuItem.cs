using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Models
{
    public class MenuItem
    {
        public int MenuItemId { get; set; }

        [Required]
        public string Name { get; set; }
        public decimal Price { get; set; }

        public bool Status { get; set; }

        public string? ImagePath { get; set; }

        public int MenuCategoryId { get; set; }
        public MenuCategory? MenuCategory { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }

        public ICollection<InventoryUsage>? InventoryUsages { get; set; }
    }
}