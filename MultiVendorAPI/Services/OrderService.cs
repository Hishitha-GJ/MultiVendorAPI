using Microsoft.EntityFrameworkCore;
using MultiVendorAPI.Data;
using MultiVendorAPI.DTOs;
using MultiVendorAPI.Interfaces;
using MultiVendorAPI.Models;
using MultiVendorAPI.Models.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MultiVendorAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly ECommerceDBContext _context;
        private readonly ICartService _cartService;
        private readonly IWebHostEnvironment _env;

        public OrderService(ECommerceDBContext context, ICartService cartService, IWebHostEnvironment env)
        {
            _context = context;
            _cartService = cartService;
            _env = env;
        }

        public async Task<int> CreateOrderAsync(int userId)
        {
            var cartItems = await _cartService.GetCartItemsAsync(userId);
            if (!cartItems.Any()) throw new Exception("Cart is empty.");

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Paid
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            await _context.SaveChangesAsync();
            await _cartService.ClearCartAsync(userId);

            return order.OrderId;
        }

        public async Task<List<OrderResponseDTO>> GetOrdersAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Select(o => new OrderResponseDTO
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    Items = o.OrderItems.Select(oi => new CartResponseDTO
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product!.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                }).ToListAsync();
        }

        public async Task<string> GenerateInvoiceAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) throw new Exception("Order not found.");

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Content().Column(column =>
                    {
                        column.Item().Text($"Invoice - Order #{order.OrderId}").FontSize(20).Bold();
                        column.Item().Text($"Customer: {order.User!.Username} | Date: {order.OrderDate}");

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Product");
                                header.Cell().Text("Price");
                                header.Cell().Text("Qty");
                                header.Cell().Text("Total");
                            });

                            foreach (var item in order.OrderItems)
                            {
                                table.Cell().Text(item.Product!.Name);
                                table.Cell().Text(item.Price.ToString("C"));
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text((item.Price * item.Quantity).ToString("C"));
                            }
                        });

                        column.Item().Text($"Grand Total: {order.OrderItems.Sum(i => i.Price * i.Quantity):C}")
                               .FontSize(16)
                               .Bold();
                    });
                });
            });

            var bytes = pdf.GeneratePdf();

            // Save invoice to wwwroot/uploads/invoices
            var folderPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "invoices");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, $"Invoice_Order_{order.OrderId}.pdf");
            await File.WriteAllBytesAsync(filePath, bytes);

            // Return relative URL
            return $"/uploads/invoices/Invoice_Order_{order.OrderId}.pdf";
        }

        public async Task<int> GetUserOrderCountAsync(int userId)
        {
            return await _context.Orders.CountAsync(o => o.UserId == userId);
        }

        public async Task<decimal> GetUserTotalSpentAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .SelectMany(o => o.OrderItems)
                .SumAsync(oi => oi.Quantity * oi.Price);
        }

        public async Task<int> GetVendorOrderCountAsync(int vendorId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Product)
                .CountAsync(oi => oi.Product!.VendorId == vendorId);
        }

        public async Task<decimal> GetVendorTotalSalesAsync(int vendorId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.Product!.VendorId == vendorId)
                .SumAsync(oi => oi.Quantity * oi.Price);
        }
    }
}
