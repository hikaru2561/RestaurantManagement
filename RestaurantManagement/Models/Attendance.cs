namespace RestaurantManagement.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }

        public int StaffId { get; set; }
        public Staffs? Staff { get; set; }

        public int ShiftId { get; set; }
        public Shift? Shift { get; set; }

        public DateTime Date { get; set; }

        public bool IsPresent { get; set; }
        public bool IsApproved { get; set; }
    }
}