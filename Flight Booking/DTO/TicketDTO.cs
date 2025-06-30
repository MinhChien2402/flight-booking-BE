using System.Text.Json.Serialization;

namespace Flight_Booking.DTO
{
    public class TicketDTO
    {
        public int Id { get; set; }
        public int AirlineId { get; set; }
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public int PlaneId { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public int Stops { get; set; }
        public decimal Price { get; set; }
        [JsonPropertyName("flight_class")]
        public string FlightClass { get; set; }
        public int AvailableSeats { get; set; }
        public DateTime? LastUpdate { get; set; }
        public decimal? DynamicPrice { get; set; }
    }
}
