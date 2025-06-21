using System.ComponentModel.DataAnnotations;

namespace Flight_Booking.Model
{
    public class ReservationHistory
    {
        [Key]
        public int HistoryId { get; set; }
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }
        public string ActionType { get; set; }
        public DateTime? OldDate { get; set; }
        public DateTime? NewDate { get; set; }
        public decimal? RefundAmount { get; set; }
        public DateTime ActionDate { get; set; }
    }
}
