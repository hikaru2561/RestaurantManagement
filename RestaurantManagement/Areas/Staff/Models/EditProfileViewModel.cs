using Microsoft.AspNetCore.Mvc;

namespace RestaurantManagement.Areas.Staff.Models
{
    public class EditProfileViewModel
    {
        public int StaffId { get; set; } // vẫn cần để kiểm tra quyền
        public string Name { get; set; }
        public string Phone { get; set; }
    }
}
