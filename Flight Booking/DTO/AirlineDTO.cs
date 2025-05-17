namespace Flight_Booking.DTO
{
    public class AirlineDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; } // Chỉ cần CountryId
        public string Callsign { get; set; }
        public string Status { get; set; }
        public List<AirlinePlaneDTO> AirlinePlanes { get; set; }
    }
}
