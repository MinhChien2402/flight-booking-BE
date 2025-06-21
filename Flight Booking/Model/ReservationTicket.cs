using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flight_Booking.Model
{
    [Table("ReservationTickets")]
    public class ReservationTicket
    {
        [Key]
        public int Id { get; set; } 

        public int ReservationId { get; set; }
        [ForeignKey("ReservationId")]
        public Reservation Reservation { get; set; }

        public int FlightScheduleId { get; set; } 
        [ForeignKey("FlightScheduleId")]
        public FlightSchedule FlightSchedule { get; set; } 
    }
}
