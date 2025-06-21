using Flight_Booking.Model;

namespace Flight_Booking.DTO
{
    public class TicketResponseDTO
    {
        public int Id { get; set; }
        public int AirlineId { get; set; }
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public int PlaneId { get; set; } // Giữ nguyên PlaneId vì nó là ID, không phải thực thể
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public int Stops { get; set; }
        public decimal BasePrice { get; set; }
        public decimal AdultPrice { get; set; }
        public decimal ChildPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string FlightClass { get; set; }
        public int AvailableSeats { get; set; }
        public DateTime? LastUpdate { get; set; }
        public decimal? DynamicPrice { get; set; }
        public Airline Airline { get; set; }
        public Airport DepartureAirport { get; set; }
        public Airport ArrivalAirport { get; set; }
        public Aircraft Aircraft { get; set; } // Thay Plane thành Aircraft
    }
}
