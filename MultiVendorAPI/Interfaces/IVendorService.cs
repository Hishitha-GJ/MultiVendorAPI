using MultiVendorAPI.DTOs;
using MultiVendorAPI.Models;

namespace MultiVendorAPI.Interfaces
{
    public interface IVendorService
    {
        Task<Vendor> RegisterVendorAsync(int userId); 
        Task<List<Vendor>> GetAllVendorsAsync();
        Task ApproveVendorAsync(int vendorId);
        Task RejectVendorAsync(int vendorId);
        Task<Vendor?> GetVendorByUserIdAsync(int userId);
        Task<bool> UpdateVendorByUserIdAsync(int userId, VendorUpdateDTO vendorUpdateDTO);

    }
}
