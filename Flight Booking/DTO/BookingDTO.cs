namespace Flight_Booking.DTO
{
    public class BookingDTO
    {
        public int? OutboundTicketId { get; set; }
        public int? ReturnTicketId { get; set; }
        public float TotalPrice { get; set; }
        public List<PassengerDTO> Passengers { get; set; }
        public string ConfirmationNumber { get; set; }
        public string ReservationStatus { get; set; }
    }

    public class PassengerDTO
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportExpiry { get; set; }
    }
}
