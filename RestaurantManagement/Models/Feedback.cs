using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public string? Content { get; set; }

        public int Rating { get; set; } // từ 1 đến 5

        public string? ImagePath { get; set; }

        public Reply? Reply { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime FeedbackTime { get; set; } = DateTime.Now;
    }
}
