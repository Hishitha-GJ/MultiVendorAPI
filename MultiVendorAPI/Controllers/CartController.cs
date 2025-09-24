using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiVendorAPI.DTOs;
using MultiVendorAPI.Interfaces;
using System.Security.Claims;

namespace MultiVendorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService) => _cartService = cartService;

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetCart()
        {
            var items = await _cartService.GetCartItemsAsync(GetUserId());
            return Ok(items);
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemDTO dto)
        {
            await _cartService.AddToCartAsync(GetUserId(), dto);
            return Ok(new { message = "Added to cart" });
        }

        [HttpPut("{cartItemId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateCart(int cartItemId, [FromBody] int quantity)
        {
            await _cartService.UpdateCartItemAsync(GetUserId(), cartItemId, quantity);
            return Ok(new { message = "Cart updated" });
        }

        [HttpDelete("{cartItemId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RemoveCart(int cartItemId)
        {
            await _cartService.RemoveCartItemAsync(GetUserId(), cartItemId);
            return Ok(new { message = "Cart item removed" });
        }

        [HttpDelete("clear")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearCartAsync(GetUserId());
            return Ok(new { message = "Cart cleared" });
        }
    }
}
