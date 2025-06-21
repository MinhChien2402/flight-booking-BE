using System.ComponentModel.DataAnnotations.Schema;

namespace Flight_Booking.Model
{
    [Table("airline_planes")]
    public class AirlineAircraft
    {
        [Column("airline_id")]
        public int AirlineId { get; set; }

        [Column("aircraft_id")] // Cập nhật cột thành aircraft_id
        public int AircraftId { get; set; } // Thay PlaneId thành AircraftId

        public Airline Airline { get; set; }
        public Aircraft Aircraft { get; set; }
    }
}