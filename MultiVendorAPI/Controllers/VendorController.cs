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
    public class VendorController : ControllerBase
    {
        private readonly IVendorService _vendorService;

        public VendorController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        private int GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claim == null) throw new Exception("UserId not found in token.");
            return int.Parse(claim);
        }

        // Vendor Registration (Customer -> Vendor)
        [Authorize(Roles = "Vendor")]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterVendor()
        {
            var userId = GetUserId();
            var vendor = await _vendorService.RegisterVendorAsync(userId);
            return Ok(vendor);
        }

        // Admin: Get All Vendors
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllVendors()
        {
            var vendors = await _vendorService.GetAllVendorsAsync();
            return Ok(vendors);
        }

        // Admin: Approve Vendor
        [Authorize(Roles = "Admin")]
        [HttpPost("{vendorId}/approve")]
        public async Task<IActionResult> ApproveVendor(int vendorId)
        {
            await _vendorService.ApproveVendorAsync(vendorId);
            return Ok(new { message = "Vendor approved." });
        }

        // Admin: Reject Vendor
        [Authorize(Roles = "Admin")]
        [HttpPost("{vendorId}/reject")]
        public async Task<IActionResult> RejectVendor(int vendorId)
        {
            await _vendorService.RejectVendorAsync(vendorId);
            return Ok(new { message = "Vendor rejected." });
        }
        [HttpGet("me")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetVendorProfile()
        {
           
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var vendor = await _vendorService.GetVendorByUserIdAsync(userId);
            if (vendor == null) return NotFound("Vendor not found");

            return Ok(new
            {
                vendor.VendorId,
                vendor.BusinessName,
                vendor.Description,
                vendor.Status
            });
        }


        [HttpPut("update")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> UpdateVendor([FromBody] VendorUpdateDTO vendorUpdateDTO)
        {
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var result = await _vendorService.UpdateVendorByUserIdAsync(userId, vendorUpdateDTO);
            if (!result) return NotFound("Vendor not found");

            return Ok("Vendor details updated successfully");
        }


    }
}
