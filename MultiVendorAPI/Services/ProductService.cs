using Microsoft.EntityFrameworkCore;
using MultiVendorAPI.Data;
using MultiVendorAPI.DTOs;
using MultiVendorAPI.Interfaces;
using MultiVendorAPI.Models;

namespace MultiVendorAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ECommerceDBContext _context;
        public ProductService(ECommerceDBContext context) => _context = context;

        public async Task<ProductResponseDTO> CreateProductAsync(int vendorId, ProductDTO dto)
        {
            var product = new Product
            {
                VendorId = vendorId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return await GetProductByIdAsync(product.ProductId) ?? throw new Exception("Product creation failed");
        }

        public async Task<ProductResponseDTO> UpdateProductAsync(int vendorId, int productId, ProductDTO dto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId && p.VendorId == vendorId);
            if (product == null) throw new Exception("Product not found or unauthorized");

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.ImageUrl = dto.ImageUrl;

            await _context.SaveChangesAsync();

            return await GetProductByIdAsync(product.ProductId) ?? throw new Exception("Product update failed");
        }

        public async Task DeleteProductAsync(int vendorId, int productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId && p.VendorId == vendorId);
            if (product == null) throw new Exception("Product not found or unauthorized");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<ProductResponseDTO?> GetProductByIdAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Vendor)
                .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null) return null;

            return new ProductResponseDTO
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                VendorName = product.Vendor?.User?.Username ?? "Unknown"
            };
        }

        public async Task<List<ProductResponseDTO>> GetAllProductsAsync(string? search = null)
        {
            var query = _context.Products.Include(p => p.Vendor).ThenInclude(v => v.User).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            return await query.Select(p => new ProductResponseDTO
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                VendorName = p.Vendor!.User!.Username
            }).ToListAsync();
        }
        public async Task<List<ProductResponseDTO>> SearchProductsAsync(string? name, decimal? minPrice, decimal? maxPrice, string? sortBy)
        {
            var query = _context.Products.Include(p => p.Vendor).ThenInclude(v => v.User).AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.Name.Contains(name) || p.Description.Contains(name));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // Sorting
            query = sortBy?.ToLower() switch
            {
                "name" => query.OrderBy(p => p.Name),
                "price" => query.OrderBy(p => p.Price),
                _ => query.OrderBy(p => p.ProductId)
            };

            return await query.Select(p => new ProductResponseDTO
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                VendorName = p.Vendor!.User!.Username
            }).ToListAsync();
        }

        public async Task<int> GetProductCountAsync(string? search = null)
        {
            var query = _context.Products.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            return await query.CountAsync();
        }

        public async Task<int> GetVendorProductCountAsync(int vendorId)
        {
            return await _context.Products.CountAsync(p => p.VendorId == vendorId);
        }

    }
}
