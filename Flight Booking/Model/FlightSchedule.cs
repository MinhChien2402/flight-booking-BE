namespace Flight_Booking.Model
{
    public class FlightSchedule
    {
        public int Id { get; set; }
        public int AirlineId { get; set; }
        public Airline Airline { get; set; }
        public int DepartureAirportId { get; set; }
        public Airport DepartureAirport { get; set; }
        public int ArrivalAirportId { get; set; }
        public Airport ArrivalAirport { get; set; }
        public int AircraftId { get; set; }
        public Aircraft Aircraft { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public int Stops { get; set; }
        public decimal Price { get; set; }
        public string FlightClass { get; set; }
        public int AvailableSeats { get; set; }
        public DateTime? LastUpdate { get; set; }
        public decimal? DynamicPrice { get; set; }
        public double? Distance { get; set; }
    }
}
