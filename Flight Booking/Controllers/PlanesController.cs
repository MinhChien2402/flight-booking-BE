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
        public class PlanesController : ControllerBase
        {
            private readonly AppDbContext _context;

            public PlanesController(AppDbContext context)
            {
                _context = context;
            }

            [HttpGet]
            public async Task<ActionResult<IEnumerable<Plane>>> GetPlanes()
            {
                return await _context.Planes.ToListAsync();
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<Plane>> GetPlane(int id)
            {
                var plane = await _context.Planes.FindAsync(id);
                if (plane == null) return NotFound();
                return plane;
            }

            [HttpPost]
            public async Task<ActionResult<Plane>> CreatePlane(Plane plane)
            {
                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(plane.Name) || plane.Name.Length > 100)
                {
                    return BadRequest("The aircraft name is invalid or exceeding 100 characters.");
                }
                if (string.IsNullOrWhiteSpace(plane.Code) || plane.Code.Length != 4)
                {
                    return BadRequest("The aircraft code must have 4 characters.");
                }
                if (!string.IsNullOrEmpty(plane.AdditionalCode) && plane.AdditionalCode.Length != 3)
                {
                    return BadRequest("Additional code must have 3 characters.");
                }

                // Kiểm tra trùng lặp Code
                var codeExists = await _context.Planes.AnyAsync(p => p.Code.ToUpper() == plane.Code.ToUpper());
                if (codeExists)
                {
                    return BadRequest("The aircraft code has existed.");
                }

                _context.Planes.Add(plane);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetPlane), new { id = plane.Id }, plane);
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> UpdatePlane(int id, Plane plane)
            {
                if (id != plane.Id)
                {
                    return BadRequest("ID does not match.");
                }

                var existingPlane = await _context.Planes.FindAsync(id);
                if (existingPlane == null)
                {
                    return NotFound();
                }

                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(plane.Name) || plane.Name.Length > 100)
                {
                    return BadRequest("The aircraft name is invalid or exceeding 100 characters.");
                }
                if (string.IsNullOrWhiteSpace(plane.Code) || plane.Code.Length != 4)
                {
                    return BadRequest("The aircraft code must have 4 characters.");
                }
                if (!string.IsNullOrEmpty(plane.AdditionalCode) && plane.AdditionalCode.Length != 3)
                {
                    return BadRequest("Additional code must have 3 characters.");
                }

                // Kiểm tra trùng lặp Code (ngoại trừ máy bay hiện tại)
                if (existingPlane.Code.ToUpper() != plane.Code.ToUpper())
                {
                    var codeExists = await _context.Planes.AnyAsync(p => p.Code.ToUpper() == plane.Code.ToUpper() && p.Id != id);
                    if (codeExists)
                    {
                        return BadRequest("The aircraft code has existed.");
                    }
                }

                _context.Entry(existingPlane).CurrentValues.SetValues(plane);
                await _context.SaveChangesAsync();
                return NoContent();
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> DeletePlane(int id)
            {
                var plane = await _context.Planes.FindAsync(id);
                if (plane == null)
                {
                    return NotFound();
                }

                // Kiểm tra ràng buộc từ airline_planes
                var hasRelatedAirlinePlanes = await _context.AirlinePlanes.AnyAsync(ap => ap.PlaneId == id);
                if (hasRelatedAirlinePlanes)
                {
                    return BadRequest("It is impossible to delete aircraft due to the airline associated with this aircraft.");
                }

                try
                {
                    _context.Planes.Remove(plane);
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
                catch (DbUpdateException)
                {
                    return BadRequest("Can not delete aircraft due to relevant data.");
                }
            }
        }
    }

}
