namespace Flight_Booking.DTO
{
    public class FlightScheduleResponseDTO
    {
        public int Id { get; set; }
        public int AirlineId { get; set; }
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public int PlaneId { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalTime { get; set; }
        public int Stops { get; set; }
        public decimal Price { get; set; }
        public string FlightClass { get; set; }
        public int AvailableSeats { get; set; }
        public string LastUpdate { get; set; }
        public decimal? DynamicPrice { get; set; }
        public AirlineDTO Airline { get; set; }
        public AirportDTO DepartureAirport { get; set; }
        public AirportDTO ArrivalAirport { get; set; }
        public AircraftDTO Aircraft { get; set; }
    }

    public class AirlineDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class AirportDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class AircraftDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
