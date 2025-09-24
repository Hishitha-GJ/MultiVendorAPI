using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultiVendorAPI.Data;
using MultiVendorAPI.DTOs;
using MultiVendorAPI.Interfaces;
using MultiVendorAPI.Models;
using MultiVendorAPI.Models.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MultiVendorAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ECommerceDBContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ECommerceDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDTO)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDTO.Email))
                throw new Exception("Email already exists.");

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == registerDTO.Role);
            if (role == null) throw new Exception("Invalid role.");

            var user = new User
            {
                Username = registerDTO.Username,
                Email = registerDTO.Email,
                RoleId = role.RoleId,
                Status = UserStatus.Active,
                PasswordHash = registerDTO.Password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Vendor profile
            if (role.Name == "Vendor")
            {
                var vendor = new Vendor { UserId = user.UserId, Status = VendorStatus.Pending };
                _context.Vendors.Add(vendor);
                await _context.SaveChangesAsync();
            }

            var token = GenerateJwtToken(user, role.Name);

            return new AuthResponseDTO { Token = token, Role = role.Name, Username = user.Username };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == loginDTO.Email);

            if (user == null || user.PasswordHash != loginDTO.Password)
                throw new Exception("Invalid credentials.");

            if (user.Status != UserStatus.Active)
                throw new Exception("User is not active.");

            if (user.Role!.Name == "Vendor")
            {
                var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.UserId == user.UserId);
                if (vendor == null || vendor.Status != VendorStatus.Approved)
                    throw new Exception("Vendor registration is pending approval by admin.");
            }

            var token = GenerateJwtToken(user, user.Role.Name);
            return new AuthResponseDTO { Token = token, Role = user.Role.Name, Username = user.Username };
        }

        private string GenerateJwtToken(User user, string role)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]);
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var duration = int.Parse(_configuration["JwtSettings:DurationInMinutes"] ?? "60");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, role)
            };

            if (role == "Vendor")
            {
                var vendor = _context.Vendors.FirstOrDefault(v => v.UserId == user.UserId);
                if (vendor != null)
                    claims.Add(new Claim("vendorId", vendor.VendorId.ToString()));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(duration),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
    }
}
