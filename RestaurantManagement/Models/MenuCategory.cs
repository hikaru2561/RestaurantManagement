namespace RestaurantManagement.Models
{
    public class MenuCategory
    {
        public int MenuCategoryId { get; set; }

        public string Name { get; set; }

        public ICollection<MenuItem>? MenuItems { get; set; }
    }
}