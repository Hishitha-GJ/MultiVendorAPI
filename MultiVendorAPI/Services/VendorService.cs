using Microsoft.EntityFrameworkCore;
using MultiVendorAPI.Data;
using MultiVendorAPI.DTOs;
using MultiVendorAPI.Interfaces;
using MultiVendorAPI.Models;
using MultiVendorAPI.Models.Enums;

namespace MultiVendorAPI.Services
{
    public class VendorService : IVendorService
    {
        private readonly ECommerceDBContext _context;
        public VendorService(ECommerceDBContext context) => _context = context;

        // Register vendor without documents
        public async Task<Vendor> RegisterVendorAsync(int userId)
        {
            var vendor = new Vendor
            {
                UserId = userId,
                Status = VendorStatus.Pending
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();
            return vendor;
        }

        // Get all vendors (no documents)
        public async Task<List<Vendor>> GetAllVendorsAsync()
        {
            return await _context.Vendors
                .Include(v => v.User)
                .ToListAsync();
        }

        public async Task ApproveVendorAsync(int vendorId)
        {
            var vendor = await _context.Vendors.FindAsync(vendorId);
            if (vendor == null) throw new Exception("Vendor not found.");
            vendor.Status = VendorStatus.Approved;
            await _context.SaveChangesAsync();
        }

        public async Task RejectVendorAsync(int vendorId)
        {
            var vendor = await _context.Vendors.FindAsync(vendorId);
            if (vendor == null) throw new Exception("Vendor not found.");
            vendor.Status = VendorStatus.Rejected;
            await _context.SaveChangesAsync();
        }
        public async Task<Vendor?> GetVendorByUserIdAsync(int userId)
        {
            return await _context.Vendors
                .FirstOrDefaultAsync(v => v.UserId == userId);
        }

        public async Task<bool> UpdateVendorByUserIdAsync(int userId, VendorUpdateDTO dto)
        {
            var vendor = await GetVendorByUserIdAsync(userId);
            if (vendor == null) return false;

            vendor.BusinessName = dto.BusinessName;
            vendor.Description = dto.Description;

            _context.Vendors.Update(vendor);
            await _context.SaveChangesAsync();
            return true;
        }


    }
}
