using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flight_Booking.Data;
using Flight_Booking.Model;
using Flight_Booking.DTO;

namespace Flight_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AirlineController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AirlineController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Airline>>> GetAirlines()
        {
            try
            {
                var airlines = await _context.Airline
                    .Include(a => a.Country)
                    .Where(a => a.CountryId.HasValue) // Lọc các bản ghi có CountryId không NULL
                    .ToListAsync();
                return Ok(airlines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAirlines: {ex.ToString()}");
                return StatusCode(500, new { message = "Error fetching airlines", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Airline>> GetAirline(int id)
        {
            try
            {
                var airline = await _context.Airline
                    .Include(a => a.Country)
                    .FirstOrDefaultAsync(a => a.Id == id);
                if (airline == null || !airline.CountryId.HasValue)
                    return NotFound(new { message = "Airline not found or invalid data" });
                return airline;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAirline: {ex.ToString()}");
                return StatusCode(500, new { message = "Error fetching airline", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Airline>> CreateAirline(AirlineDto airlineDto)
        {
            var country = await _context.Countries.FindAsync(airlineDto.CountryId);
            if (country == null) return BadRequest("Invalid CountryId. Country does not exist.");

            var airline = new Airline
            {
                Name = airlineDto.Name,
                CountryId = airlineDto.CountryId,
                Callsign = airlineDto.Callsign,
                Status = airlineDto.Status,
                AirlinePlanes = airlineDto.AirlinePlanes?.Select(p => new AirlineAircraft
                {
                    AirlineId = p.AirlineId,
                    AircraftId = p.AircraftId
                }).ToList() ?? new List<AirlineAircraft>()
            };

            _context.Airline.Add(airline);
            await _context.SaveChangesAsync();

            var createdAirline = await _context.Airline
                .Include(a => a.Country)
                .FirstOrDefaultAsync(a => a.Id == airline.Id);

            return CreatedAtAction(nameof(GetAirline), new { id = airline.Id }, createdAirline);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAirline(int id, AirlineDto airlineDto)
        {
            if (id != airlineDto.Id) return BadRequest();

            var airline = await _context.Airline.Include(a => a.AirlinePlanes).FirstOrDefaultAsync(a => a.Id == id);
            if (airline == null) return NotFound();

            var country = await _context.Countries.FindAsync(airlineDto.CountryId);
            if (country == null) return BadRequest("Invalid CountryId. Country does not exist.");

            airline.Name = airlineDto.Name;
            airline.CountryId = airlineDto.CountryId;
            airline.Callsign = airlineDto.Callsign;
            airline.Status = airlineDto.Status;
            if (airlineDto.AirlinePlanes != null)
            {
                airline.AirlinePlanes = airlineDto.AirlinePlanes.Select(p => new AirlineAircraft
                {
                    AirlineId = p.AirlineId,
                    AircraftId = p.AircraftId
                }).ToList();
            }

            _context.Entry(airline).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAirline(int id)
        {
            var airline = await _context.Airline.FindAsync(id);
            if (airline == null) return NotFound();

            var relatedAirlinePlanes = await _context.AirlinePlanes
                .Where(ap => ap.AirlineId == id)
                .ToListAsync();
            if (relatedAirlinePlanes.Any())
            {
                _context.AirlinePlanes.RemoveRange(relatedAirlinePlanes);
            }

            _context.Airline.Remove(airline);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}