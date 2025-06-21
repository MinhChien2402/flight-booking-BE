using System.ComponentModel.DataAnnotations.Schema;

namespace Flight_Booking.Model
{
    [Table("Country")]
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("additional_code")]
        public string AdditionalCode { get; set; }
    }
}
