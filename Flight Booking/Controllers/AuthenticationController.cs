// src/Controllers/AuthenticationController.cs
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
using Microsoft.EntityFrameworkCore;

namespace Flight_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(new { message = "Validation errors", errors });
            }

            // Chỉ kiểm tra các trường bắt buộc
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email and Password are required." });
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User already exists" });
            }

            var hashedPassword = HashPasswordMD5(request.Password);
            var user = new User
            {
                FullName = string.IsNullOrEmpty(request.FullName) ? null : request.FullName,
                Email = request.Email,
                Password = hashedPassword,
                Role = request.Role ?? "customer",
                PhoneNumber = string.IsNullOrEmpty(request.PhoneNumber) ? null : request.PhoneNumber,
                Address = string.IsNullOrEmpty(request.Address) ? null : request.Address,
                PreferredCreditCard = string.IsNullOrEmpty(request.PreferredCreditCard) ? null : request.PreferredCreditCard,
                Sex = string.IsNullOrEmpty(request.Sex) ? null : request.Sex,
                Age = request.Age ?? 0,
                SkyMiles = 0 // Mặc định khi đăng ký
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

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid email or password" });
            }

            if (string.IsNullOrEmpty(user.Password) || HashPasswordMD5(request.Password) != user.Password)
            {
                return BadRequest(new { message = "Invalid email or password" });
            }

            var userDto = new UserDTO
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = user.Role ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Address = user.Address ?? string.Empty,
                Sex = user.Sex ?? string.Empty,
                Age = user.Age,
                PreferredCreditCard = user.PreferredCreditCard ?? string.Empty,
                SkyMiles = user.SkyMiles
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
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Sử dụng Id thay vì UserId
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
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}