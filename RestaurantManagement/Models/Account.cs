using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Models
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }  
    }
}
