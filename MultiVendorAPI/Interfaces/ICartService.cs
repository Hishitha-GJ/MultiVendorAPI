using MultiVendorAPI.DTOs;

namespace MultiVendorAPI.Interfaces
{
    public interface ICartService
    {
        Task<List<CartResponseDTO>> GetCartItemsAsync(int userId);
        Task AddToCartAsync(int userId, CartItemDTO dto);
        Task UpdateCartItemAsync(int userId, int cartItemId, int quantity);
        Task RemoveCartItemAsync(int userId, int cartItemId);
        Task ClearCartAsync(int userId);
    }
}
