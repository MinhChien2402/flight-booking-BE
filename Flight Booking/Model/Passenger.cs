using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flight_Booking.Model
{
    [Table("Passenger")]
    public class Passenger
    {
        [Key]
        public int Id { get; set; } // Thay PassengerId thành Id

        public int ReservationId { get; set; } // Thay BookingId thành ReservationId để khớp với Reservation
        [ForeignKey("ReservationId")]
        public Reservation Reservation { get; set; } // Thay Booking thành Reservation

        public string Title { get; set; }
        [Column("first_name")]
        public string FirstName { get; set; }
        [Column("last_name")]
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [Column("passport_number")]
        public string PassportNumber { get; set; }
        [Column("passport_expiry")]
        public DateTime? PassportExpiry { get; set; }
    }
}
