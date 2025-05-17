namespace Flight_Booking.DTO
{
    public class BookingDTO
    {
        public int TicketId { get; set; } // ID của vé/chuyến bay
        public decimal TotalPrice { get; set; }
        public List<PassengerInfo> Passengers { get; set; }
    }

    public class PassengerInfo
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PassportNumber { get; set; }
        public DateTime PassportExpiry { get; set; }
    }
}
