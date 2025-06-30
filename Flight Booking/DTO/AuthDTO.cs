using System.ComponentModel.DataAnnotations;

namespace Flight_Booking.DTO
{
    public class AuthDTO
    {
        public class RegisterRequest
        {
            [Required]
            public string FullName { get; set; }
            [Required]
            public string Email { get; set; }
            [Required]
            public string Password { get; set; }
            public string? PhoneNumber { get; set; }
            public string Role { get; set; }
            public string? Address { get; set; }
            public string? Sex { get; set; }
            public int? Age { get; set; }
            public string? PreferredCreditCard { get; set; }
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class AuthResponse
        {
            public string Message { get; set; }
            public string Token { get; set; }
            public UserDTO User { get; set; }
        }

        public class UserDTO
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public string Sex { get; set; }
            public int? Age { get; set; }
            public string PreferredCreditCard { get; set; }
            public decimal SkyMiles { get; set; }
        }

        public class UpdateUserRequest
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string Address { get; set; }
            public string Sex { get; set; }
            public int? Age { get; set; }
            public string PreferredCreditCard { get; set; }
        }
    }
}
