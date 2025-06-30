namespace Flight_Booking.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Flight_Booking.Data;
    using Flight_Booking.Model;
    using Microsoft.Extensions.Logging;

    [Route("api/[controller]")]
    [ApiController]
    public class AirportController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AirportController> _logger;

        public AirportController(AppDbContext context, ILogger<AirportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Airport>>> GetAirports()
        {
            try
            {
                var airports = await _context.Airports.ToListAsync();
                if (airports == null || !airports.Any())
                {
                    _logger.LogWarning("No airports found in database.");
                    return new List<Airport>(); // Trả về danh sách rỗng thay vì null
                }
                _logger.LogInformation("Retrieved {Count} airports: {@Airports}", airports.Count, airports);
                return airports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving airports.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Airport>> GetAirport(int id)
        {
            var airport = await _context.Airports.FindAsync(id);
            if (airport == null)
            {
                _logger.LogWarning("Airport with ID {Id} not found.", id);
                return NotFound();
            }
            _logger.LogInformation("Retrieved airport with ID {Id}: {@Airport}", id, airport);
            return airport;
        }

        [HttpPost]
        public async Task<ActionResult<Airport>> CreateAirport(Airport airport)
        {
            try
            {
                _context.Airports.Add(airport);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created airport with ID {Id}: {@Airport}", airport.Id, airport);
                return CreatedAtAction(nameof(GetAirport), new { id = airport.Id }, airport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating airport.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAirport(int id, Airport airport)
        {
            try
            {
                airport.Id = id;

                if (id != airport.Id) return BadRequest("ID trong URL và body không khớp.");

                var existingAirport = await _context.Airports.FindAsync(id);
                if (existingAirport == null) return NotFound();

                var codeExists = await _context.Airports.AnyAsync(a => a.Code.ToUpper() == airport.Code.ToUpper() && a.Id != id);
                if (codeExists) return BadRequest("Mã sân bay đã tồn tại.");

                _context.Entry(existingAirport).CurrentValues.SetValues(airport);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated airport with ID {Id}: {@Airport}", id, airport);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating airport with ID {Id}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAirport(int id)
        {
            try
            {
                var airport = await _context.Airports.FindAsync(id);
                if (airport == null) return NotFound();

                _context.Airports.Remove(airport);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted airport with ID {Id}.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting airport with ID {Id}.", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}