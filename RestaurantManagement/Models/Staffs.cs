using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Models
{
    public class Staffs
    {
        [Key]
        public int StaffId { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Name { get; set; }


        public string Phone { get; set; }
        public string? ImagePath { get; set; }

        public ICollection<Shift>? Shifts { get; set; }
        public ICollection<Attendance>? Attendances { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}