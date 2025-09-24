using MultiVendorAPI.DTOs;

namespace MultiVendorAPI.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponseDTO> CreateProductAsync(int vendorId, ProductDTO dto);
        Task<ProductResponseDTO> UpdateProductAsync(int vendorId, int productId, ProductDTO dto);
        Task DeleteProductAsync(int vendorId, int productId);
        Task<ProductResponseDTO?> GetProductByIdAsync(int productId);
        Task<List<ProductResponseDTO>> GetAllProductsAsync(string? search = null);
        Task<List<ProductResponseDTO>> SearchProductsAsync(string? name, decimal? minPrice, decimal? maxPrice, string? sortBy);
        Task<int> GetProductCountAsync(string? search = null);
        Task<int> GetVendorProductCountAsync(int vendorId);

    }
}
