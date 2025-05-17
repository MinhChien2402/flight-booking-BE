namespace Flight_Booking.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Flight_Booking.Data;
    using Flight_Booking.Model;

    namespace AirlineAPI.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class AirlinePlanesController : ControllerBase
        {
            private readonly AppDbContext _context;

            public AirlinePlanesController(AppDbContext context)
            {
                _context = context;
            }

            [HttpGet]
            public async Task<ActionResult<IEnumerable<AirlinePlane>>> GetAirlinePlanes()
            {
                return await _context.AirlinePlanes
                    .Include(ap => ap.Airline)
                    .Include(ap => ap.Plane)
                    .ToListAsync();
            }

            [HttpGet("{airlineId}/{planeId}")]
            public async Task<ActionResult<AirlinePlane>> GetAirlinePlane(int airlineId, int planeId)
            {
                var airlinePlane = await _context.AirlinePlanes
                    .Include(ap => ap.Airline)
                    .Include(ap => ap.Plane)
                    .FirstOrDefaultAsync(ap => ap.AirlineId == airlineId && ap.PlaneId == planeId);

                if (airlinePlane == null) return NotFound();
                return airlinePlane;
            }

            [HttpPost]
            public async Task<ActionResult<AirlinePlane>> CreateAirlinePlane(AirlinePlane airlinePlane)
            {
                _context.AirlinePlanes.Add(airlinePlane);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAirlinePlane),
                    new { airlineId = airlinePlane.AirlineId, planeId = airlinePlane.PlaneId },
                    airlinePlane);
            }

            [HttpDelete("{airlineId}/{planeId}")]
            public async Task<IActionResult> DeleteAirlinePlane(int airlineId, int planeId)
            {
                var airlinePlane = await _context.AirlinePlanes
                    .FirstOrDefaultAsync(ap => ap.AirlineId == airlineId && ap.PlaneId == planeId);

                if (airlinePlane == null) return NotFound();

                _context.AirlinePlanes.Remove(airlinePlane);
                await _context.SaveChangesAsync();

                return NoContent();
            }
        }
    }

}
