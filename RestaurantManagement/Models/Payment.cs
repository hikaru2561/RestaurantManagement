namespace RestaurantManagement.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime PaymentTime { get; set; }

        public string Method { get; set; } // Tiền mặt / Thẻ
    }
}