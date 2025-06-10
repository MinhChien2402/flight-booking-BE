using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flight_Booking.Model
{
    public class BookingTicket
    {
        [Key]
        public int BookingTicketId { get; set; } // Khóa chính riêng
        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }
        public int TicketId { get; set; }
        [ForeignKey("TicketId")]
        public Tickets Ticket { get; set; }
    }
}
