using Microsoft.EntityFrameworkCore;
using MultiVendorAPI.Data;
using MultiVendorAPI.DTOs;
using MultiVendorAPI.Interfaces;
using MultiVendorAPI.Models;

namespace MultiVendorAPI.Services
{
    public class CartService : ICartService
    {
        private readonly ECommerceDBContext _context;
        public CartService(ECommerceDBContext context) => _context = context;

        public async Task<List<CartResponseDTO>> GetCartItemsAsync(int userId)
        {
            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .Select(c => new CartResponseDTO
                {
                    CartItemId = c.CartItemId,
                    ProductId = c.ProductId,
                    ProductName = c.Product!.Name,
                    Price = c.Product.Price,
                    Quantity = c.Quantity
                }).ToListAsync();
        }

        public async Task AddToCartAsync(int userId, CartItemDTO dto)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == dto.ProductId);

            if (cartItem != null)
            {
                cartItem.Quantity += dto.Quantity;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(int userId, int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.CartItemId == cartItemId && c.UserId == userId);
            if (cartItem == null) throw new Exception("Cart item not found.");
            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveCartItemAsync(int userId, int cartItemId)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.CartItemId == cartItemId && c.UserId == userId);
            if (cartItem == null) throw new Exception("Cart item not found.");
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(int userId)
        {
            var cartItems = _context.CartItems.Where(c => c.UserId == userId);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }
    }
}
