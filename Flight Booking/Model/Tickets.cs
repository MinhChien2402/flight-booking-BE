namespace Flight_Booking.Model
{
    public class Tickets
    {
        public int Id { get; set; }
        public int AirlineId { get; set; }
        public Airline Airline { get; set; } // Liên kết với hãng hàng không
        public int DepartureAirportId { get; set; }
        public Airport DepartureAirport { get; set; } // Sân bay khởi hành
        public int ArrivalAirportId { get; set; }
        public Airport ArrivalAirport { get; set; } // Sân bay điểm đến
        public int PlaneId { get; set; }
        public Plane Plane { get; set; } // Máy bay
        public DateTime DepartureTime { get; set; } // Thời gian khởi hành
        public DateTime ArrivalTime { get; set; } // Thời gian đến
        public int Stops { get; set; } // Số điểm dừng
        public decimal Price { get; set; } // Giá vé
        public string FlightClass { get; set; } // Loại ghế (Economy, Business, v.v.)
        public int AvailableSeats { get; set; } // Số ghế còn lại
    }
}
