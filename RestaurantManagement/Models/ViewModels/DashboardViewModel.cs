namespace RestaurantManagement.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TodayOrderCount { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public int TodayReservationCount { get; set; }
        public double AverageRating { get; set; }

        public List<MenuItemStat> TopMenuItems { get; set; } = new();
        public List<DailyRevenue> WeeklyRevenue { get; set; } = new();
    }

    public class MenuItemStat
    {
        public string Name { get; set; } = "";
        public int TotalSold { get; set; }
    }

    public class DailyRevenue
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
    }
}
