using System.ComponentModel.DataAnnotations.Schema;

namespace Flight_Booking.Model
{
    public class Airport
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        [Column("additional_code")]
        public string AdditionalCode { get; set; }
    }
}
