using Flight_Booking.Model;

namespace Flight_Booking.DTO
{
    public class TicketResponseDTO
    {
        public int Id { get; set; }
        public int AirlineId { get; set; }
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public int PlaneId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int Stops { get; set; }
        public decimal BasePrice { get; set; } // Giá cơ bản (người lớn)
        public decimal AdultPrice { get; set; } // Giá cho tất cả người lớn
        public decimal ChildPrice { get; set; } // Giá cho tất cả trẻ em
        public decimal TotalPrice { get; set; } // Tổng giá
        public string FlightClass { get; set; }
        public int AvailableSeats { get; set; }
        public Airline Airline { get; set; }
        public Airport DepartureAirport { get; set; }
        public Airport ArrivalAirport { get; set; }
        public Plane Plane { get; set; }
    }
}
