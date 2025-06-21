namespace Flight_Booking.DTO
{
    public class TicketSearchDTO
    {
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string TripType { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public string FlightClass { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}
