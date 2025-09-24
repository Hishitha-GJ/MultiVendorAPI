using MultiVendorAPI.DTOs;

namespace MultiVendorAPI.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO);
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDTO);
    }
}
