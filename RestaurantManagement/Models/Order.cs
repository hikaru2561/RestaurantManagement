namespace RestaurantManagement.Models
{
    public enum OrderStatus
    {
        Ordered = 0,
        Preparing = 1,
        Canceled = 2,
        Paid = 3,
        Completed = 4
    }

    public class Order
    {
        public int OrderId { get; set; }

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int TableId { get; set; }
        public Table? Table { get; set; }

        public DateTime OrderTime { get; set; }

        public int StaffId { get; set; }
        public Staffs? Staff { get; set; }

        public OrderStatus Status { get; set; } // Đã gọi / Đang làm / Huỷ / Hoàn tất

        public ICollection<OrderItem>? OrderItems { get; set; }
        public Payment? Payment { get; set; }

        public Feedback? Feedback { get; set; }  // 1-1 Feedback cho 1 Order
    }
}
