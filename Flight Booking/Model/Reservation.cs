using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace Flight_Booking.Model
{
    [Table("Reservation")]
    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime ReservationDate { get; set; }
        public string ReservationStatus { get; set; }
        public decimal TotalFare { get; set; }
        public DateTime? BlockExpiryDate { get; set; }
        public string ConfirmationNumber { get; set; } 
        public string CancellationRules { get; set; } = "Default rules";
        public int? FlightScheduleId { get; set; } // Thêm thuộc tính mới
        [ForeignKey("FlightScheduleId")]
        public FlightSchedule FlightSchedule { get; set; } // Thêm navigation property
        public List<Passenger> Passengers { get; set; } = new List<Passenger>();
        public List<ReservationTicket> ReservationTickets { get; set; } = new List<ReservationTicket>();
    }
}

