using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Username { get; set; }

        [Required]
        public string Name { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }

        public bool IsVIP { get; set; }

        public ICollection<Reservation>? Reservations { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
