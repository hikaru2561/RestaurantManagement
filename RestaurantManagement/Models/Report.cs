using System;

namespace RestaurantManagement.Models
{
    public class Report
    {
        public int ReportId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }  // Có thể lưu HTML, Markdown hoặc plain text

        public DateTime CreatedAt { get; set; }

        public string? CreatedBy { get; set; }  // Tên nhân viên hoặc hệ thống
    }
}
