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
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService) => _productService = productService;

        private int GetVendorId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "vendorId")?.Value;
            if (claim == null) throw new Exception("VendorId not found in token.");
            return int.Parse(claim);
        }

        [Authorize(Roles = "Vendor")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDTO dto) =>
            Ok(await _productService.CreateProductAsync(GetVendorId(), dto));

        [Authorize(Roles = "Vendor")]
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] ProductDTO dto) =>
            Ok(await _productService.UpdateProductAsync(GetVendorId(), productId, dto));

        [Authorize(Roles = "Vendor")]
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            await _productService.DeleteProductAsync(GetVendorId(), productId);
            return Ok(new { message = "Product deleted" });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] string? search = null) =>
            Ok(await _productService.GetAllProductsAsync(search));

        [AllowAnonymous]
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProduct(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null) return NotFound();
            return Ok(product);
        }
        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string? name, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] string? sortBy)
        {
            var products = await _productService.SearchProductsAsync(name, minPrice, maxPrice, sortBy);
            return Ok(products);
        }

        [AllowAnonymous]
        [HttpGet("count")]
        public async Task<IActionResult> GetProductCount([FromQuery] string? search)
        {
            var count = await _productService.GetProductCountAsync(search);
            return Ok(new { count });
        }

        [Authorize(Roles = "Vendor")]
        [HttpGet("vendor/count")]
        public async Task<IActionResult> GetVendorProductCount()
        {
            var vendorId = int.Parse(User.Claims.First(c => c.Type == "vendorId").Value);
            var count = await _productService.GetVendorProductCountAsync(vendorId);
            return Ok(new { count });
        }

    }
}
