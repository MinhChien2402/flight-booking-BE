namespace Flight_Booking.DTO
{
    public class TicketSearchDTO
    {
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public DateTime DepartureDate { get; set; }
    }
}
