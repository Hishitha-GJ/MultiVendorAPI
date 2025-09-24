namespace MultiVendorAPI.DTOs
{
    public class ProductResponseDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
    }
}
