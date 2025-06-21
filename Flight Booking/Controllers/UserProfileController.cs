using Flight_Booking.Data;
using Flight_Booking.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Flight_Booking.Model;
using static Flight_Booking.DTO.AuthDTO;
using System.Security.Claims;

namespace Flight_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserProfileController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userDto = new
            {
                fullName = user.FullName ?? string.Empty,
                email = user.Email ?? string.Empty,
                phoneNumber = user.PhoneNumber ?? string.Empty,
                dateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty,
                address = user.Address ?? string.Empty,
                sex = user.Sex ?? string.Empty,
                age = user.Age,
                preferredCreditCard = user.PreferredCreditCard ?? string.Empty,
                skyMiles = user.SkyMiles
            };

            return Ok(userDto);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            if (string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "FullName and Email are required" });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            if (userId != user.Id)
            {
                return StatusCode(403, new { message = "You are not authorized to update this user" }); // Thay Forbid
            }

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.DateOfBirth = request.DateOfBirth;
            user.Address = request.Address;
            user.Sex = request.Sex;
            user.Age = request.Age;
            user.PreferredCreditCard = request.PreferredCreditCard;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
        }
    }
}