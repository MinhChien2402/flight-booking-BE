using System.Net.Sockets;

namespace Flight_Booking.Model
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public List<Passenger> Passengers { get; set; } = new List<Passenger>();
        public List<BookingTicket> BookingTickets { get; set; } = new List<BookingTicket>();
    }
}
