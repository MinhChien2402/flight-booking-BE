using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flight_Booking.DTO
{
    public class BookingDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int? OutboundTicketId { get; set; }

        public int? ReturnTicketId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; } // Thay float bằng decimal cho độ chính xác

        [Required]
        public List<PassengerDTO> Passengers { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 5)] // Giới hạn độ dài mã xác nhận
        public string ConfirmationNumber { get; set; }

        [Required]
        [StringLength(100)] // Giới hạn độ dài quy định hủy vé
        public string ReservationStatus { get; set; }

        [Required]
        [StringLength(200)]
        public string CancellationRules { get; set; }
    }

    public class PassengerDTO
    {
        [Required]
        [StringLength(10)] // Giới hạn độ dài, ví dụ: Mr, Ms
        public string Title { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        [StringLength(20)]
        public string PassportNumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? PassportExpiry { get; set; }
    }
}