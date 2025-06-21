using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flight_Booking.Data;
using Flight_Booking.Model;

namespace Flight_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AircraftController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AircraftController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Aircraft>>> GetAircrafts()
        {
            return await _context.Aircrafts.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Aircraft>> GetAircraft(int id)
        {
            var aircraft = await _context.Aircrafts.FindAsync(id);
            if (aircraft == null) return NotFound();
            return aircraft;
        }

        [HttpPost]
        public async Task<ActionResult<Aircraft>> CreateAircraft(Aircraft aircraft)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(aircraft.Name) || aircraft.Name.Length > 100)
            {
                return BadRequest(new { message = "The aircraft name is invalid or exceeding 100 characters." });
            }
            if (string.IsNullOrWhiteSpace(aircraft.Code) || aircraft.Code.Length != 4)
            {
                return BadRequest(new { message = "The aircraft code must have 4 characters." });
            }
            if (!string.IsNullOrEmpty(aircraft.AdditionalCode) && aircraft.AdditionalCode.Length != 3)
            {
                return BadRequest(new { message = "Additional code must have 3 characters." });
            }

            // Kiểm tra trùng lặp Code
            var codeExists = await _context.Aircrafts.AnyAsync(p => p.Code.ToUpper() == aircraft.Code.ToUpper());
            if (codeExists)
            {
                return BadRequest(new { message = "The aircraft code has existed." });
            }

            _context.Aircrafts.Add(aircraft);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAircraft), new { id = aircraft.Id }, aircraft);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAircraft(int id, Aircraft aircraft)
        {
            if (id != aircraft.Id)
            {
                return BadRequest(new { message = "ID does not match." });
            }

            var existingAircraft = await _context.Aircrafts.FindAsync(id);
            if (existingAircraft == null)
            {
                return NotFound();
            }

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(aircraft.Name) || aircraft.Name.Length > 100)
            {
                return BadRequest(new { message = "The aircraft name is invalid or exceeding 100 characters." });
            }
            if (string.IsNullOrWhiteSpace(aircraft.Code) || aircraft.Code.Length != 4)
            {
                return BadRequest(new { message = "The aircraft code must have 4 characters." });
            }
            if (!string.IsNullOrEmpty(aircraft.AdditionalCode) && aircraft.AdditionalCode.Length != 3)
            {
                return BadRequest(new { message = "Additional code must have 3 characters." });
            }

            // Kiểm tra trùng lặp Code (ngoại trừ máy bay hiện tại)
            if (existingAircraft.Code.ToUpper() != aircraft.Code.ToUpper())
            {
                var codeExists = await _context.Aircrafts.AnyAsync(p => p.Code.ToUpper() == aircraft.Code.ToUpper() && p.Id != id);
                if (codeExists)
                {
                    return BadRequest(new { message = "The aircraft code has existed." });
                }
            }

            _context.Entry(existingAircraft).CurrentValues.SetValues(aircraft);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAircraft(int id)
        {
            var aircraft = await _context.Aircrafts.FindAsync(id);
            if (aircraft == null)
            {
                return NotFound();
            }

            // Kiểm tra ràng buộc từ AirlineAircraft
            var hasRelatedAirlineAircrafts = await _context.AirlinePlanes.AnyAsync(ap => ap.AircraftId == id);
            if (hasRelatedAirlineAircrafts)
            {
                return BadRequest(new { message = "It is impossible to delete aircraft due to the airline associated with this aircraft." });
            }

            try
            {
                _context.Aircrafts.Remove(aircraft);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = "Cannot delete aircraft due to relevant data.", error = ex.Message });
            }
        }
    }
}