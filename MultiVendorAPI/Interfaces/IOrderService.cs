using MultiVendorAPI.DTOs;

namespace MultiVendorAPI.Interfaces
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(int userId);
        Task<List<OrderResponseDTO>> GetOrdersAsync(int userId);
        Task<string> GenerateInvoiceAsync(int orderId); 
        Task<int> GetUserOrderCountAsync(int userId);
        Task<decimal> GetUserTotalSpentAsync(int userId);
        Task<int> GetVendorOrderCountAsync(int vendorId);
        Task<decimal> GetVendorTotalSalesAsync(int vendorId);
    }
}
