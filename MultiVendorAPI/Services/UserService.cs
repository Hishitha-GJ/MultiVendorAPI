using Microsoft.EntityFrameworkCore;
using MultiVendorAPI.Data;
using MultiVendorAPI.DTOs;
using MultiVendorAPI.Interfaces;
using MultiVendorAPI.Models;

namespace MultiVendorAPI.Services
{
    public class UserService : IUserService
    {
        private readonly ECommerceDBContext _context;

        public UserService(ECommerceDBContext context)
        {
            _context = context;
        }

        public async Task<List<UserDTO>> GetAllUsersAsync(string? search = null, string? role = null)
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search));

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role!.Name == role);

            return await query.Select(u => new UserDTO
            {
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role!.Name,
                Status = u.Status.ToString()
            }).ToListAsync();
        }

        public async Task<UserDTO?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return null;

            return new UserDTO
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role!.Name,
                Status = user.Status.ToString()
            };
        }

        public async Task<UserDTO> CreateUserAsync(UserDTO dto)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role);
            if (role == null) throw new Exception("Invalid role");

            var user = new Models.User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = dto.Password, // plain string password
                RoleId = role.RoleId,
                Status = Enum.Parse<Models.Enums.UserStatus>(dto.Status)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            dto.UserId = user.UserId;
            return dto;
        }

        public async Task<UserDTO> UpdateUserAsync(int userId, UserDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) throw new Exception("User not found");

            user.Username = dto.Username;
            user.Email = dto.Email;
            user.PasswordHash = dto.Password; // plain string password
            if (!string.IsNullOrEmpty(dto.Role))
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role);
                if (role != null) user.RoleId = role.RoleId;
            }
            if (!string.IsNullOrEmpty(dto.Status))
                user.Status = Enum.Parse<Models.Enums.UserStatus>(dto.Status);

            await _context.SaveChangesAsync();

            return dto;
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) throw new Exception("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUserCountAsync(string? role = null)
        {
            var query = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role!.Name == role);

            return await query.CountAsync();
        }
    }
}
