using MultiVendorAPI.DTOs;
using MultiVendorAPI.Models;

namespace MultiVendorAPI.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDTO>> GetAllUsersAsync(string? search = null, string? role = null);
        Task<UserDTO?> GetUserByIdAsync(int userId);
        Task<UserDTO> CreateUserAsync(UserDTO dto);
        Task<UserDTO> UpdateUserAsync(int userId, UserDTO dto);
        Task DeleteUserAsync(int userId);
        Task<int> GetUserCountAsync(string? role = null);
    }
}
