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
        public async Task<ActionResult<object>> SearchTickets([FromBody] TicketSearchDTO searchDto)
        {
            // Log dữ liệu nhận được để debug
            Console.WriteLine("Received search params: " + System.Text.Json.JsonSerializer.Serialize(searchDto));

            // Kiểm tra dữ liệu đầu vào
            if (searchDto == null)
            {
                return BadRequest("Request body is missing or invalid.");
            }

            // Kiểm tra các trường bắt buộc
            if (searchDto.DepartureAirportId <= 0 || searchDto.ArrivalAirportId <= 0)
            {
                return BadRequest("DepartureAirportId và ArrivalAirportId phải hợp lệ.");
            }
            if (searchDto.DepartureDate == null)
            {
                return BadRequest("DepartureDate là bắt buộc.");
            }
            if (searchDto.Adults <= 0 && searchDto.Children <= 0)
            {
                return BadRequest("Phải có ít nhất 1 hành khách.");
            }
            if (string.IsNullOrEmpty(searchDto.FlightClass))
            {
                return BadRequest("FlightClass là bắt buộc.");
            }

            // Chuẩn hóa TripType
            searchDto.TripType = string.IsNullOrEmpty(searchDto.TripType) ? "oneWay" : searchDto.TripType.ToLower();
            if (searchDto.TripType != "oneway" && searchDto.TripType != "roundtrip")
            {
                return BadRequest("TripType phải là 'oneWay' hoặc 'roundTrip'.");
            }

            // Chuẩn hóa FlightClass
            searchDto.FlightClass = string.IsNullOrEmpty(searchDto.FlightClass) ? "Economy" : searchDto.FlightClass;

            // Chuẩn hóa ngày giờ về UTC
            var departureDateUtc = searchDto.DepartureDate.Value.ToUniversalTime().Date;
            var returnDateUtc = searchDto.ReturnDate.HasValue ? searchDto.ReturnDate.Value.ToUniversalTime().Date : (DateTime?)null;

            Console.WriteLine($"DepartureDate (UTC): {departureDateUtc}, ReturnDate (UTC): {returnDateUtc}");

            try
            {
                // Truy vấn vé outbound
                var outboundTickets = await _context.Tickets
                    .Include(t => t.Airline)
                    .Include(t => t.DepartureAirport)
                    .Include(t => t.ArrivalAirport)
                    .Include(t => t.Plane)
                    .Where(t => t.DepartureAirportId == searchDto.DepartureAirportId
                             && t.ArrivalAirportId == searchDto.ArrivalAirportId
                             && t.DepartureTime.Date == departureDateUtc
                             && t.FlightClass == searchDto.FlightClass
                             && t.AvailableSeats >= (searchDto.Adults + searchDto.Children))
                    .ToListAsync();

                var outboundResponse = outboundTickets.Select(t =>
                {
                    decimal adultPrice = t.Price * searchDto.Adults;
                    decimal childPrice = t.Price * 0.4m * searchDto.Children;
                    decimal totalPrice = adultPrice + childPrice;

                    return new TicketResponseDTO
                    {
                        Id = t.Id,
                        AirlineId = t.AirlineId,
                        DepartureAirportId = t.DepartureAirportId,
                        ArrivalAirportId = t.ArrivalAirportId,
                        PlaneId = t.PlaneId,
                        DepartureTime = t.DepartureTime,
                        ArrivalTime = t.ArrivalTime,
                        Stops = t.Stops,
                        BasePrice = t.Price,
                        AdultPrice = adultPrice,
                        ChildPrice = childPrice,
                        TotalPrice = totalPrice,
                        FlightClass = t.FlightClass,
                        AvailableSeats = t.AvailableSeats,
                        Airline = t.Airline,
                        DepartureAirport = t.DepartureAirport,
                        ArrivalAirport = t.ArrivalAirport,
                        Plane = t.Plane
                    };
                }).ToList();

                // Truy vấn vé return
                List<TicketResponseDTO> returnTickets = new List<TicketResponseDTO>();
                if (searchDto.TripType == "roundtrip" && returnDateUtc.HasValue)
                {
                    var returnTicketsQuery = await _context.Tickets
                        .Include(t => t.Airline)
                        .Include(t => t.DepartureAirport)
                        .Include(t => t.ArrivalAirport)
                        .Include(t => t.Plane)
                        .Where(t => t.DepartureAirportId == searchDto.ArrivalAirportId
                                 && t.ArrivalAirportId == searchDto.DepartureAirportId
                                 && t.DepartureTime.Date == returnDateUtc.Value
                                 && t.FlightClass == searchDto.FlightClass
                                 && t.AvailableSeats >= (searchDto.Adults + searchDto.Children))
                        .ToListAsync();

                    returnTickets = returnTicketsQuery.Select(t =>
                    {
                        decimal adultPrice = t.Price * searchDto.Adults;
                        decimal childPrice = t.Price * 0.4m * searchDto.Children;
                        decimal totalPrice = adultPrice + childPrice;

                        return new TicketResponseDTO
                        {
                            Id = t.Id,
                            AirlineId = t.AirlineId,
                            DepartureAirportId = t.DepartureAirportId,
                            ArrivalAirportId = t.ArrivalAirportId,
                            PlaneId = t.PlaneId,
                            DepartureTime = t.DepartureTime,
                            ArrivalTime = t.ArrivalTime,
                            Stops = t.Stops,
                            BasePrice = t.Price,
                            AdultPrice = adultPrice,
                            ChildPrice = childPrice,
                            TotalPrice = totalPrice,
                            FlightClass = t.FlightClass,
                            AvailableSeats = t.AvailableSeats,
                            Airline = t.Airline,
                            DepartureAirport = t.DepartureAirport,
                            ArrivalAirport = t.ArrivalAirport,
                            Plane = t.Plane
                        };
                    }).ToList();
                }

                var result = new
                {
                    OutboundTickets = outboundResponse,
                    ReturnTickets = returnTickets,
                    Message = (outboundResponse.Count == 0 && returnTickets.Count == 0)
                        ? "Không tìm thấy vé phù hợp. Có thể do ngày khởi hành không có chuyến bay hoặc không đủ ghế trống."
                        : null
                };

                Console.WriteLine("Found tickets: " + System.Text.Json.JsonSerializer.Serialize(result));
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching tickets: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, "Lỗi server khi tìm kiếm vé. Vui lòng thử lại sau.");
            }
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

            // Sử dụng trực tiếp giá trị từ DTO
            DateTime departureTime = ticketDto.DepartureTime;
            DateTime arrivalTime = ticketDto.ArrivalTime;

            // Kiểm tra logic nghiệp vụ
            if (departureTime >= arrivalTime)
            {
                return BadRequest("Departure time must be before the arrival time.");
            }
            if (ticketDto.Price < 0)
            {
                return BadRequest("The ticket price cannot be negative.");
            }
            if (ticketDto.AvailableSeats < 0)
            {
                return BadRequest("The number of seats cannot be negative.");
            }

            var ticket = new Tickets
            {
                AirlineId = ticketDto.AirlineId,
                DepartureAirportId = ticketDto.DepartureAirportId,
                ArrivalAirportId = ticketDto.ArrivalAirportId,
                PlaneId = ticketDto.PlaneId,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
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
                return BadRequest("The airline does not exist.");
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
                return BadRequest("The arrival airport does not exist.");
            }

            // Kiểm tra logic nghiệp vụ
            if (ticketDto.DepartureTime >= ticketDto.ArrivalTime)
            {
                return BadRequest("Departure time must be before the arrival time.");
            }
            if (ticketDto.Price < 0)
            {
                return BadRequest("The ticket price cannot be negative.");
            }
            if (ticketDto.AvailableSeats < 0)
            {
                return BadRequest("The number of seats cannot be negative.");
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
                    return NotFound($"No plane found for airlineId {airlineId}.");
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