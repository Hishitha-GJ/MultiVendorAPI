using MultiVendorAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiVendorAPI.Models
{
    public class Vendor
    {
        [Key]
        public int VendorId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Required, MaxLength(200)]
        public string BusinessName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public VendorStatus Status { get; set; } = VendorStatus.Pending;

        public ICollection<Product>? Products { get; set; }
    }
}
