using System;
using System.Collections.Generic;

namespace MultiVendorAPI.DTOs
{
    public class OrderResponseDTO
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;

        // List of items in this order
        public List<CartResponseDTO> Items { get; set; } = new List<CartResponseDTO>();
    }
}
