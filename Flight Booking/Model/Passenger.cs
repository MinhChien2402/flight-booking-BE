using System.ComponentModel.DataAnnotations.Schema;

namespace Flight_Booking.Model
{
    public class Passenger
    {
        public int PassengerId { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
        public string Title { get; set; }
        [Column("first_name")]
        public string FirstName { get; set; }
        [Column("last_name")]
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        [Column("passport_number")]
        public string PassportNumber { get; set; }
        [Column("passport_expiry")]
        public DateTime PassportExpiry { get; set; }
    }
}
