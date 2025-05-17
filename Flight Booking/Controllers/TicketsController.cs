using Flight_Booking.Data;
using Flight_Booking.DTO;
using Flight_Booking.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flight_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketsController(AppDbContext context)
        {
            _context = context;
        }

        // API tìm kiếm vé máy bay
        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<Tickets>>> SearchTickets(
    [FromBody] TicketSearchDTO searchDto)
        {
            Console.WriteLine("Received search params: " + System.Text.Json.JsonSerializer.Serialize(searchDto));

            var tickets = await _context.Tickets
                .Include(t => t.Airline)
                .Include(t => t.DepartureAirport)
                .Include(t => t.ArrivalAirport)
                .Include(t => t.Plane)
                .Where(t => t.DepartureAirportId == searchDto.DepartureAirportId
                         && t.ArrivalAirportId == searchDto.ArrivalAirportId
                         && t.DepartureTime.Date == searchDto.DepartureDate.Date)
                .ToListAsync();

            Console.WriteLine("Found tickets: " + System.Text.Json.JsonSerializer.Serialize(tickets));
            return Ok(tickets);
        }

        // API lấy danh sách tất cả vé
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tickets>>> GetTickets()
        {
            return await _context.Tickets
                .Include(t => t.Airline)
                .Include(t => t.DepartureAirport)
                .Include(t => t.ArrivalAirport)
                .Include(t => t.Plane)
                .ToListAsync();
        }

        // API tạo vé mới
        [HttpPost]
        public async Task<ActionResult<Tickets>> CreateTicket(TicketDTO ticketDto)
        {
            // Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra sự tồn tại của các khóa ngoại
            if (!await _context.Airlines.AnyAsync(a => a.Id == ticketDto.AirlineId))
            {
                return BadRequest("Airlines do not exist.");
            }
            if (!await _context.Planes.AnyAsync(p => p.Id == ticketDto.PlaneId))
            {
                return BadRequest("The aircraft does not exist.");
            }
            if (!await _context.Airports.AnyAsync(a => a.Id == ticketDto.DepartureAirportId))
            {
                return BadRequest("The departure airport does not exist.");
            }
            if (!await _context.Airports.AnyAsync(a => a.Id == ticketDto.ArrivalAirportId))
            {
                return BadRequest("The airport does not exist.");
            }

            // Kiểm tra logic nghiệp vụ
            if (ticketDto.DepartureTime >= ticketDto.ArrivalTime)
            {
                return BadRequest("Departure time must be before the next time.");
            }
            if (ticketDto.Price < 0)
            {
                return BadRequest("The ticket price is not negative.");
            }
            if (ticketDto.AvailableSeats < 0)
            {
                return BadRequest("The number of seats is not negative.");
            }

            var ticket = new Tickets
            {
                AirlineId = ticketDto.AirlineId,
                DepartureAirportId = ticketDto.DepartureAirportId,
                ArrivalAirportId = ticketDto.ArrivalAirportId,
                PlaneId = ticketDto.PlaneId,
                DepartureTime = ticketDto.DepartureTime,
                ArrivalTime = ticketDto.ArrivalTime,
                Stops = ticketDto.Stops,
                Price = ticketDto.Price,
                FlightClass = ticketDto.FlightClass,
                AvailableSeats = ticketDto.AvailableSeats,
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
        }

        // API lấy thông tin một vé
        [HttpGet("{id}")]
        public async Task<ActionResult<Tickets>> GetTicket(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Airline)
                .Include(t => t.DepartureAirport)
                .Include(t => t.ArrivalAirport)
                .Include(t => t.Plane)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();
            return ticket;
        }

        // API cập nhật vé
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, TicketDTO ticketDto)
        {
            if (id != ticketDto.Id) return BadRequest("ID does not match.");

            var existingTicket = await _context.Tickets.FindAsync(id);
            if (existingTicket == null) return NotFound();

            // Kiểm tra sự tồn tại của các khóa ngoại
            if (!await _context.Airlines.AnyAsync(a => a.Id == ticketDto.AirlineId))
            {
                return BadRequest("The aircraft does not exist.");
            }
            if (!await _context.Planes.AnyAsync(p => p.Id == ticketDto.PlaneId))
            {
                return BadRequest("The aircraft does not exist.");
            }
            if (!await _context.Airports.AnyAsync(a => a.Id == ticketDto.DepartureAirportId))
            {
                return BadRequest("The departure airport does not exist.");
            }
            if (!await _context.Airports.AnyAsync(a => a.Id == ticketDto.ArrivalAirportId))
            {
                return BadRequest("The airport does not exist.");
            }

            // Kiểm tra logic nghiệp vụ
            if (ticketDto.DepartureTime >= ticketDto.ArrivalTime)
            {
                return BadRequest("Departure time must be before the next time.");
            }
            if (ticketDto.Price < 0)
            {
                return BadRequest("The ticket price is not negative.");
            }
            if (ticketDto.AvailableSeats < 0)
            {
                return BadRequest("The number of seats is not negative.");
            }

            existingTicket.AirlineId = ticketDto.AirlineId;
            existingTicket.DepartureAirportId = ticketDto.DepartureAirportId;
            existingTicket.ArrivalAirportId = ticketDto.ArrivalAirportId;
            existingTicket.PlaneId = ticketDto.PlaneId;
            existingTicket.DepartureTime = ticketDto.DepartureTime;
            existingTicket.ArrivalTime = ticketDto.ArrivalTime;
            existingTicket.Stops = ticketDto.Stops;
            existingTicket.Price = ticketDto.Price;
            existingTicket.FlightClass = ticketDto.FlightClass;
            existingTicket.AvailableSeats = ticketDto.AvailableSeats;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // API xóa vé
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // API mới: Lấy danh sách máy bay theo airlineId
        [HttpGet("planes/by-airline/{airlineId}")]
        public async Task<ActionResult<IEnumerable<Plane>>> GetPlanesByAirline(int airlineId)
        {
            try
            {
                var planes = await _context.AirlinePlanes
                    .Where(ap => ap.AirlineId == airlineId)
                    .Select(ap => ap.Plane)
                    .ToListAsync();

                if (planes == null || !planes.Any())
                {
                    return NotFound($"No plane found airlineId {airlineId}.");
                }

                return Ok(planes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}