namespace RestaurantManagement.Models
{
    public class InventoryUsage
    {
        public int InventoryUsageId { get; set; }

        public int InventoryItemId { get; set; }
       
        public int MenuItemId { get; set; }

        public int QuantityUsed { get; set; }

        // Navigation
        public InventoryItem? InventoryItem { get; set; }
        public MenuItem? MenuItem { get; set; }
    }
}
