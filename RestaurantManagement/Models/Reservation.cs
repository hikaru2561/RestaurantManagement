using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Models
{
    public enum ReservationStatus
    {
        Pending, // Chưa đến
        Confirmed, // Đã đến
        Canceled
    }

    public class Reservation
    {
        public int ReservationId { get; set; }

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int DingningTableId { get; set; }
        public DingningTable? DingningTable { get; set; }

        public DateTime ReservationTime { get; set; }

        public int NumberOfPeople { get; set; }

        public decimal DepositAmount { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    }

}