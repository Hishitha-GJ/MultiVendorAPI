using MultiVendorAPI.Models.Enums;

namespace MultiVendorAPI.DTOs
{
    public class VendorResponseDTO
    {
        public int VendorId { get; set; }
        public int UserId { get; set; }
        public VendorStatus Status { get; set; }
    }
}
