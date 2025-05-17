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
    public class AirlinesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AirlinesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Airline>>> GetAirlines()
        {
            return await _context.Airlines.Include(a => a.Country).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Airline>> GetAirline(int id)
        {
            var airline = await _context.Airlines.Include(a => a.Country)
                                                .FirstOrDefaultAsync(a => a.Id == id);
            if (airline == null) return NotFound();
            return airline;
        }

        [HttpPost]
        public async Task<ActionResult<Airline>> CreateAirline(AirlineDto airlineDto)
        {
            // Kiểm tra CountryId
            var country = await _context.Countries.FindAsync(airlineDto.CountryId);
            if (country == null)
            {
                return BadRequest("Invalid CountryId. Country does not exist.");
            }

            // Ánh xạ từ DTO sang model Airline
            var airline = new Airline
            {
                Name = airlineDto.Name,
                CountryId = airlineDto.CountryId,
                Callsign = airlineDto.Callsign,
                Status = airlineDto.Status,
                AirlinePlanes = airlineDto.AirlinePlanes?.Select(p => new AirlinePlane
                {
                    AirlineId = p.AirlineId,
                    PlaneId = p.PlaneId
                }).ToList() ?? new List<AirlinePlane>()
            };

            _context.Airlines.Add(airline);
            await _context.SaveChangesAsync();

            // Lấy lại airline với Country
            var createdAirline = await _context.Airlines
                .Include(a => a.Country)
                .FirstOrDefaultAsync(a => a.Id == airline.Id);

            return CreatedAtAction(nameof(GetAirline), new { id = airline.Id }, createdAirline);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAirline(int id, AirlineDto airlineDto)
        {
            if (id != airlineDto.Id) return BadRequest();

            var airline = await _context.Airlines.Include(a => a.AirlinePlanes).FirstOrDefaultAsync(a => a.Id == id);
            if (airline == null) return NotFound();

            // Kiểm tra CountryId
            var country = await _context.Countries.FindAsync(airlineDto.CountryId);
            if (country == null)
            {
                return BadRequest("Invalid CountryId. Country does not exist.");
            }

            // Cập nhật từ DTO sang model Airline
            airline.Name = airlineDto.Name;
            airline.CountryId = airlineDto.CountryId;
            airline.Callsign = airlineDto.Callsign;
            airline.Status = airlineDto.Status;
            if (airlineDto.AirlinePlanes != null)
            {
                airline.AirlinePlanes = airlineDto.AirlinePlanes.Select(p => new AirlinePlane
                {
                    AirlineId = p.AirlineId,
                    PlaneId = p.PlaneId
                }).ToList();
            }

            _context.Entry(airline).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAirline(int id)
        {
            var airline = await _context.Airlines.FindAsync(id);
            if (airline == null) return NotFound();

            // Xóa các bản ghi liên quan trong airline_planes
            var relatedAirlinePlanes = await _context.AirlinePlanes
                .Where(ap => ap.AirlineId == id)
                .ToListAsync();
            if (relatedAirlinePlanes.Any())
            {
                _context.AirlinePlanes.RemoveRange(relatedAirlinePlanes);
            }

            // Xóa airline
            _context.Airlines.Remove(airline);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}