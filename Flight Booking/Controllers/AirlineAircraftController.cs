using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flight_Booking.Data;
using Flight_Booking.Model;

namespace Flight_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirlineAircraftController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AirlineAircraftController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AirlineAircraft>>> GetAirlineAircrafts()
        {
            return await _context.AirlinePlanes
                .Include(ap => ap.Airline)
                .Include(ap => ap.Aircraft)
                .ToListAsync();
        }

        [HttpGet("{airlineId}/{aircraftId}")]
        public async Task<ActionResult<AirlineAircraft>> GetAirlineAircraft(int airlineId, int aircraftId)
        {
            var airlineAircraft = await _context.AirlinePlanes
                .Include(ap => ap.Airline)
                .Include(ap => ap.Aircraft)
                .FirstOrDefaultAsync(ap => ap.AirlineId == airlineId && ap.AircraftId == aircraftId);

            if (airlineAircraft == null) return NotFound();
            return airlineAircraft;
        }

        [HttpPost]
        public async Task<ActionResult<AirlineAircraft>> CreateAirlineAircraft(AirlineAircraft airlineAircraft)
        {
            // Kiểm tra dữ liệu đầu vào
            if (airlineAircraft == null || airlineAircraft.AirlineId <= 0 || airlineAircraft.AircraftId <= 0)
            {
                return BadRequest(new { message = "AirlineId and AircraftId are required." });
            }

            var airline = await _context.Airline.FindAsync(airlineAircraft.AirlineId);
            var aircraft = await _context.Aircrafts.FindAsync(airlineAircraft.AircraftId);
            if (airline == null || aircraft == null)
            {
                return BadRequest(new { message = "Invalid AirlineId or AircraftId." });
            }

            _context.AirlinePlanes.Add(airlineAircraft);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAirlineAircraft),
                new { airlineId = airlineAircraft.AirlineId, aircraftId = airlineAircraft.AircraftId },
                airlineAircraft);
        }

        [HttpDelete("{airlineId}/{aircraftId}")]
        public async Task<IActionResult> DeleteAirlineAircraft(int airlineId, int aircraftId)
        {
            var airlineAircraft = await _context.AirlinePlanes
                .FirstOrDefaultAsync(ap => ap.AirlineId == airlineId && ap.AircraftId == aircraftId);

            if (airlineAircraft == null) return NotFound();

            _context.AirlinePlanes.Remove(airlineAircraft);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = "Cannot delete due to related data.", error = ex.Message });
            }

            return NoContent();
        }
    }
}