namespace Flight_Booking.DTO
{
    public class TicketSearchDTO
    {
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string TripType { get; set; } // "oneWay" hoặc "roundTrip"
        public int Adults { get; set; } // Số lượng người lớn
        public int Children { get; set; } // Số lượng trẻ em
        public string FlightClass { get; set; } // Loại khoang: "Economy", "Business", v.v.
        public DateTime? ReturnDate { get; set; } // Ngày về, nullable cho vé một chiều
    }
}
