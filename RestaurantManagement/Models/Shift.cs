namespace RestaurantManagement.Models
{
    public class Shift
    {
        public int ShiftId { get; set; }

        public string Name { get; set; } // Ca sáng, chiều

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public ICollection<Attendance>? Attendances { get; set; }
    }
}