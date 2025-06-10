namespace Flight_Booking.DTO
{
    public class TicketDTO
    {
        public int Id { get; set; } // Dùng cho update, bỏ qua khi create
        public int AirlineId { get; set; }
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public int PlaneId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int Stops { get; set; }
        public decimal Price { get; set; } // Giá cơ bản cho một người lớn
        public string FlightClass { get; set; }
        public int AvailableSeats { get; set; }
    }
}
