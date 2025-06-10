namespace Flight_Booking.DTO
{
    public class BookingDTO
    {
        public int? OutboundTicketId { get; set; } // ID của vé outbound (khứ hồi)
        public int? ReturnTicketId { get; set; }   // ID của vé return (nếu có)
        public float TotalPrice { get; set; }
        public List<PassengerDTO> Passengers { get; set; }
    }

    public class PassengerDTO
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PassportNumber { get; set; }
        public DateTime PassportExpiry { get; set; }
    }
}
