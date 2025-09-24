using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiVendorAPI.Interfaces;
using System.Security.Claims;

namespace MultiVendorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IWebHostEnvironment _env;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public OrderController(IOrderService orderService, IWebHostEnvironment env)
        {
            _orderService = orderService;
            _env = env;
        }

        [HttpPost("checkout")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Checkout()
        {
            var orderId = await _orderService.CreateOrderAsync(UserId);
            return Ok(new { orderId, message = "Order created successfully" });
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _orderService.GetOrdersAsync(UserId);
            return Ok(orders);
        }

        [HttpGet("{orderId}/invoice")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetInvoice(int orderId)
        {
            // Generate invoice and get saved URL
            var invoiceUrl = await _orderService.GenerateInvoiceAsync(orderId);

            // Return URL to client
            return Ok(new { invoiceUrl });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("analytics")]
        public async Task<IActionResult> GetCustomerAnalytics()
        {
            var orderCount = await _orderService.GetUserOrderCountAsync(UserId);
            var totalSpent = await _orderService.GetUserTotalSpentAsync(UserId);
            return Ok(new { orderCount, totalSpent });
        }

        [Authorize(Roles = "Vendor")]
        [HttpGet("vendor/analytics")]
        public async Task<IActionResult> GetVendorAnalytics()
        {
            var vendorId = int.Parse(User.Claims.First(c => c.Type == "vendorId")!.Value);
            var orderCount = await _orderService.GetVendorOrderCountAsync(vendorId);
            var totalSales = await _orderService.GetVendorTotalSalesAsync(vendorId);
            return Ok(new { orderCount, totalSales });
        }
    }
}
