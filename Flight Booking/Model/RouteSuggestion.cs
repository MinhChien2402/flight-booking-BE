using System.ComponentModel.DataAnnotations;

namespace Flight_Booking.Model
{
    public class RouteSuggestion
    {
        [Key]
        public int RouteId { get; set; }
        public int DepartureAirportId { get; set; }
        public Airport DepartureAirport { get; set; }
        public int ArrivalAirportId { get; set; }
        public Airport ArrivalAirport { get; set; }
        public string TransferPoints { get; set; }
    }
}
