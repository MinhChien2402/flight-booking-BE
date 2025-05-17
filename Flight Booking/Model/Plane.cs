using System.ComponentModel.DataAnnotations.Schema;

namespace Flight_Booking.Model
{
    public class Plane
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        [Column("additional_code")]
        public string AdditionalCode { get; set; }

        // Thêm thuộc tính này để phản ánh mối quan hệ nhiều-nhiều với AirlinePlane
        public ICollection<AirlinePlane> AirlinePlanes { get; set; }
    }
}
