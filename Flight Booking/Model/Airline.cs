using System.ComponentModel.DataAnnotations.Schema;

namespace Flight_Booking.Model
{
    [Table("airlines")]
    public class Airline
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Column("country_id")]
        public int CountryId { get; set; }

        public Country Country { get; set; }
        public string Callsign { get; set; }
        public string Status { get; set; } = "Active";

        public ICollection<AirlinePlane> AirlinePlanes { get; set; }
    }
}