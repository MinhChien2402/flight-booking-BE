using System.ComponentModel.DataAnnotations.Schema;

namespace Flight_Booking.Model
{
    [Table("Aircraft")]
    public class Aircraft
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        [Column("additional_code")]
        public string AdditionalCode { get; set; }

        public ICollection<AirlineAircraft> AirlineAircrafts { get; set; } // Đổi từ AirlinePlanes thành AirlineAircrafts
    }
}
