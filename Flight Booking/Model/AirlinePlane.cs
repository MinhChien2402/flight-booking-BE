using System.ComponentModel.DataAnnotations.Schema;

namespace Flight_Booking.Model
{
    [Table("airline_planes")]
    public class AirlinePlane
    {
        [Column("airline_id")]
        public int AirlineId { get; set; }

        [Column("plane_id")]
        public int PlaneId { get; set; }

        public Airline Airline { get; set; }
        public Plane Plane { get; set; }
    }
}