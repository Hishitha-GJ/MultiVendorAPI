namespace MultiVendorAPI.DTOs
{
    public class UserDTO
    {
        public int? UserId { get; set; }       // Required for update/delete, optional for create
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!; // "Admin", "Vendor", "Customer"
        public string? Status { get; set; }    // Optional: "Active", "Inactive"
        public string? Password { get; set; }  // Optional: only when creating or resetting
    }
}
