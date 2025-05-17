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
        public class AirportsController : ControllerBase
        {
            private readonly AppDbContext _context;

            public AirportsController(AppDbContext context)
            {
                _context = context;
            }

            [HttpGet]
            public async Task<ActionResult<IEnumerable<Airport>>> GetAirports()
            {
                return await _context.Airports.ToListAsync();
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<Airport>> GetAirport(int id)
            {
                var airport = await _context.Airports.FindAsync(id);
                if (airport == null) return NotFound();
                return airport;
            }

            [HttpPost]
            public async Task<ActionResult<Airport>> CreateAirport(Airport airport)
            {
                _context.Airports.Add(airport);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetAirport), new { id = airport.Id }, airport);
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateAirport(int id, Airport airport)
            {
                // Gán id từ URL vào airport.Id
                airport.Id = id;

                if (id != airport.Id) return BadRequest("ID trong URL và body không khớp.");

                // Kiểm tra xem airport có tồn tại không
                var existingAirport = await _context.Airports.FindAsync(id);
                if (existingAirport == null) return NotFound();

                // Kiểm tra trùng lặp code (trừ chính nó)
                var codeExists = await _context.Airports.AnyAsync(a => a.Code.ToUpper() == airport.Code.ToUpper() && a.Id != id);
                if (codeExists) return BadRequest("Mã sân bay đã tồn tại.");

                // Cập nhật dữ liệu
                _context.Entry(existingAirport).CurrentValues.SetValues(airport);
                await _context.SaveChangesAsync();

                return NoContent();
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteAirport(int id)
            {
                var airport = await _context.Airports.FindAsync(id);
                if (airport == null) return NotFound();

                _context.Airports.Remove(airport);
                await _context.SaveChangesAsync();

                return NoContent();
            }
        }
    }

}
