namespace Flight_Booking.Model
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Sex { get; set; }
        public int? Age { get; set; }
        public string? PreferredCreditCard { get; set; }
        public decimal SkyMiles { get; set; } = 0;
    }
}
