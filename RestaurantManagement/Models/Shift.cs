namespace RestaurantManagement.Models
{
    public class Shift
    {
        public int ShiftId { get; set; }

        public string Name { get; set; } // Ca sáng, chiều

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public ICollection<Attendance>? Attendances { get; set; }
    }
}