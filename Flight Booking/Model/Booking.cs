using System.Net.Sockets;

namespace Flight_Booking.Model
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int TicketId { get; set; } // Thay vì FlightId, dùng TicketId để phù hợp với bảng tickets
        public Tickets Ticket { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }

        public List<Passenger> Passengers { get; set; } = new List<Passenger>();
    }
}
