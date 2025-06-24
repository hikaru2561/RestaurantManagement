using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Models
{
    public enum AttendanceStatus
    {
        Registered,    // Đã đăng ký
        Approved,      // Đã duyệt
        Present,       // Có mặt
        Absent         // Vắng
    }
    public class Attendance
    {
        public int AttendanceId { get; set; }

        public int StaffId { get; set; }
        public Staffs? Staff { get; set; }

        public int ShiftId { get; set; }
        public Shift? Shift { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Registered;
        
    }
}