namespace Flight_Booking.DTO
{
    public class AuthDTO
    {
        public class RegisterRequest
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Role { get; set; }
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
        }

        public class UpdateUserRequest
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public DateTime? DateOfBirth { get; set; }
        }
    }
}
