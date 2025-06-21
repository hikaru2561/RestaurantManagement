namespace RestaurantManagement.Models
{
    public class Reply
    {
        public int ReplyId { get; set; }

        public int FeedbackId { get; set; }
        public Feedback? Feedback { get; set; }

        public string Content { get; set; }

        public DateTime RepliedAt { get; set; } = DateTime.Now;
    }
}
