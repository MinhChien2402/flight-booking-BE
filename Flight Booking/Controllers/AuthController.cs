using Flight_Booking.Data;
using Flight_Booking.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Flight_Booking.Model;
using static Flight_Booking.DTO.AuthDTO;
using Microsoft.AspNetCore.Authorization;

namespace Flight_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "All fields are required" });
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User already exists" });
            }

            var hashedPassword = HashPasswordMD5(request.Password);
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = hashedPassword,
                Role = request.Role ?? "customer", // Sử dụng Role từ request, mặc định là customer
                PhoneNumber = request.PhoneNumber ?? "N/A"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            var user = _context.Users
                .FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid email or password" });
            }

            // Kiểm tra Password
            if (string.IsNullOrEmpty(user.Password) || HashPasswordMD5(request.Password) != user.Password)
            {
                return BadRequest(new { message = "Invalid email or password" });
            }

            // Đảm bảo các trường không NULL khi tạo UserDTO
            var userDto = new UserDTO
            {
                FullName = user.FullName ?? string.Empty, // Xử lý NULL
                Email = user.Email ?? string.Empty,     // Xử lý NULL
                Role = user.Role ?? string.Empty        // Xử lý NULL
            };

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponse
            {
                Message = "Login successful",
                Token = token,
                User = userDto
            });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPasswordMD5(string password)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(password);
                var hashBytes = md5.ComputeHash(inputBytes);
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // Chuyển thành chuỗi hex
                }
                return sb.ToString();
            }
        }
    }
}