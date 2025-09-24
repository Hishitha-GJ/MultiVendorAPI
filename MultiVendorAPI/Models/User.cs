using MultiVendorAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace MultiVendorAPI.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public Role? Role { get; set; }

        public UserStatus Status { get; set; } = UserStatus.Active;

        public Vendor? VendorProfile { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<CartItem>? CartItems { get; set; }
    }
}
