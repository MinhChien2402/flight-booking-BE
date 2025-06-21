using Flight_Booking.Data;
using Flight_Booking.DTO;
using Flight_Booking.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Flight_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightScheduleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FlightScheduleController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetFlightSchedules()
        {
            try
            {
                Console.WriteLine("Fetching flight schedules from database...");
                var flightSchedules = await _context.FlightSchedules
                    .Include(fs => fs.Airline)
                    .Include(fs => fs.DepartureAirport)
                    .Include(fs => fs.ArrivalAirport)
                    .Include(fs => fs.Aircraft)
                    .ToListAsync();

                Console.WriteLine($"Raw data count: {flightSchedules.Count}");
                foreach (var fs in flightSchedules)
                {
                    Console.WriteLine($"Schedule {fs.Id}: Airline={fs.Airline?.Name}, Departure={fs.DepartureAirport?.Name}");
                }

                var safeFlightSchedules = flightSchedules
                    .Where(fs => fs.DepartureTime != null && fs.ArrivalTime != null) // Thay HasValue bằng != null
                    .Select(fs => new
                    {
                        id = fs.Id,
                        airline_id = fs.AirlineId,
                        departure_airport_id = fs.DepartureAirportId,
                        arrival_airport_id = fs.ArrivalAirportId,
                        plane_id = fs.AircraftId,
                        departure_time = fs.DepartureTime?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "", // Xử lý nullable
                        arrival_time = fs.ArrivalTime?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "",     // Xử lý nullable
                        stops = fs.Stops,
                        price = fs.Price,
                        flight_class = fs.FlightClass,
                        available_seats = fs.AvailableSeats,
                        last_update = fs.LastUpdate?.ToString("yyyy-MM-ddTHH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                        dynamic_price = fs.DynamicPrice,
                        airline = fs.Airline != null ? new { id = fs.Airline.Id, name = fs.Airline.Name } : null,
                        departure_airport = fs.DepartureAirport != null ? new { id = fs.DepartureAirport.Id, name = fs.DepartureAirport.Name } : null,
                        arrival_airport = fs.ArrivalAirport != null ? new { id = fs.ArrivalAirport.Id, name = fs.ArrivalAirport.Name } : null,
                        aircraft = fs.Aircraft != null ? new { id = fs.Aircraft.Id, name = fs.Aircraft.Name } : null
                    }).ToList();

                Console.WriteLine($"Valid schedules count: {safeFlightSchedules.Count}");
                Console.WriteLine("Serialized response: " + JsonSerializer.Serialize(safeFlightSchedules));
                return safeFlightSchedules.Any() ? Ok(safeFlightSchedules) : NotFound(new { message = "No valid flight schedules found." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFlightSchedules: {ex.ToString()}");
                return StatusCode(500, new { message = "Error fetching flight schedules", error = ex.Message });
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<object>> SearchTickets([FromBody] TicketSearchDTO searchDto)
        {
            Console.WriteLine("Nhận tham số tìm kiếm: " + JsonSerializer.Serialize(searchDto));

            if (searchDto == null)
            {
                return BadRequest(new { message = "Dữ liệu yêu cầu không hợp lệ hoặc bị thiếu." });
            }

            if (searchDto.DepartureAirportId <= 0 || searchDto.ArrivalAirportId <= 0)
            {
                return BadRequest(new { message = "DepartureAirportId và ArrivalAirportId phải hợp lệ." });
            }
            if (!searchDto.DepartureDate.HasValue)
            {
                return BadRequest(new { message = "DepartureDate là bắt buộc." });
            }
            if (searchDto.Adults <= 0 && searchDto.Children <= 0)
            {
                return BadRequest(new { message = "Cần ít nhất 1 hành khách." });
            }
            if (string.IsNullOrEmpty(searchDto.FlightClass))
            {
                return BadRequest(new { message = "FlightClass là bắt buộc." });
            }

            searchDto.TripType = string.IsNullOrEmpty(searchDto.TripType) ? "oneWay" : searchDto.TripType.ToLower();
            if (searchDto.TripType != "oneway" && searchDto.TripType != "roundtrip")
            {
                return BadRequest(new { message = "TripType phải là 'oneWay' hoặc 'roundTrip'." });
            }

            searchDto.FlightClass = string.IsNullOrEmpty(searchDto.FlightClass) ? "Economy" : searchDto.FlightClass;

            var departureDateUtc = searchDto.DepartureDate.Value.ToUniversalTime();
            var returnDateUtc = searchDto.ReturnDate.HasValue ? searchDto.ReturnDate.Value.ToUniversalTime() : (DateTime?)null;

            Console.WriteLine($"DepartureDate (UTC): {departureDateUtc}, ReturnDate (UTC): {returnDateUtc}");

            try
            {
                var outboundTickets = await _context.FlightSchedules
                    .Include(fs => fs.Airline)
                    .Include(fs => fs.DepartureAirport)
                    .Include(fs => fs.ArrivalAirport)
                    .Include(fs => fs.Aircraft)
                    .Where(fs => fs.DepartureAirportId == searchDto.DepartureAirportId
                              && fs.ArrivalAirportId == searchDto.ArrivalAirportId
                              && fs.DepartureTime != null
                              && fs.DepartureTime.Value.Date == departureDateUtc.Date
                              && fs.FlightClass == searchDto.FlightClass
                              && fs.AvailableSeats >= (searchDto.Adults + searchDto.Children))
                    .ToListAsync();

                var outboundResponse = outboundTickets
                    .Where(fs => fs.DepartureTime != null && fs.ArrivalTime != null)
                    .Select(fs => new FlightScheduleResponseDTO
                    {
                        Id = fs.Id,
                        AirlineId = fs.AirlineId,
                        DepartureAirportId = fs.DepartureAirportId,
                        ArrivalAirportId = fs.ArrivalAirportId,
                        PlaneId = fs.AircraftId,
                        DepartureTime = fs.DepartureTime.Value.ToString("yyyy-MM-ddTHH:mm:ss"),
                        ArrivalTime = fs.ArrivalTime.Value.ToString("yyyy-MM-ddTHH:mm:ss"),
                        Stops = fs.Stops,
                        Price = fs.Price,
                        FlightClass = fs.FlightClass,
                        AvailableSeats = fs.AvailableSeats,
                        LastUpdate = fs.LastUpdate?.ToString("yyyy-MM-ddTHH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                        DynamicPrice = fs.DynamicPrice,
                        Airline = fs.Airline != null ? new AirlineDTO { Id = fs.Airline.Id, Name = fs.Airline.Name } : null,
                        DepartureAirport = fs.DepartureAirport != null ? new AirportDTO { Id = fs.DepartureAirport.Id, Name = fs.DepartureAirport.Name } : null,
                        ArrivalAirport = fs.ArrivalAirport != null ? new AirportDTO { Id = fs.ArrivalAirport.Id, Name = fs.ArrivalAirport.Name } : null,
                        Aircraft = fs.Aircraft != null ? new AircraftDTO { Id = fs.Aircraft.Id, Name = fs.Aircraft.Name } : null
                    }).ToList();

                var returnTickets = new List<FlightScheduleResponseDTO>();
                if (searchDto.TripType == "roundtrip" && returnDateUtc.HasValue)
                {
                    var returnTicketsQuery = await _context.FlightSchedules
                        .Include(fs => fs.Airline)
                        .Include(fs => fs.DepartureAirport)
                        .Include(fs => fs.ArrivalAirport)
                        .Include(fs => fs.Aircraft)
                        .Where(fs => fs.DepartureAirportId == searchDto.ArrivalAirportId
                                  && fs.ArrivalAirportId == searchDto.DepartureAirportId
                                  && fs.DepartureTime != null
                                  && fs.DepartureTime.Value.Date == returnDateUtc.Value.Date
                                  && fs.FlightClass == searchDto.FlightClass
                                  && fs.AvailableSeats >= (searchDto.Adults + searchDto.Children))
                        .ToListAsync();

                    returnTickets = returnTicketsQuery
                        .Where(fs => fs.DepartureTime != null && fs.ArrivalTime != null)
                        .Select(fs => new FlightScheduleResponseDTO
                        {
                            Id = fs.Id,
                            AirlineId = fs.AirlineId,
                            DepartureAirportId = fs.DepartureAirportId,
                            ArrivalAirportId = fs.ArrivalAirportId,
                            PlaneId = fs.AircraftId,
                            DepartureTime = fs.DepartureTime.Value.ToString("yyyy-MM-ddTHH:mm:ss"),
                            ArrivalTime = fs.ArrivalTime.Value.ToString("yyyy-MM-ddTHH:mm:ss"),
                            Stops = fs.Stops,
                            Price = fs.Price,
                            FlightClass = fs.FlightClass,
                            AvailableSeats = fs.AvailableSeats,
                            LastUpdate = fs.LastUpdate?.ToString("yyyy-MM-ddTHH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                            DynamicPrice = fs.DynamicPrice,
                            Airline = fs.Airline != null ? new AirlineDTO { Id = fs.Airline.Id, Name = fs.Airline.Name } : null,
                            DepartureAirport = fs.DepartureAirport != null ? new AirportDTO { Id = fs.DepartureAirport.Id, Name = fs.DepartureAirport.Name } : null,
                            ArrivalAirport = fs.ArrivalAirport != null ? new AirportDTO { Id = fs.ArrivalAirport.Id, Name = fs.ArrivalAirport.Name } : null,
                            Aircraft = fs.Aircraft != null ? new AircraftDTO { Id = fs.Aircraft.Id, Name = fs.Aircraft.Name } : null
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

                Console.WriteLine("Tìm thấy vé: " + JsonSerializer.Serialize(result));
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tìm kiếm vé: {ex}");
                return StatusCode(500, new { message = "Lỗi server khi tìm kiếm vé. Vui lòng thử lại sau.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<FlightSchedule>> CreateFlightSchedule(TicketDTO ticketDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _context.Airlines.AnyAsync(a => a.Id == ticketDto.AirlineId))
            {
                return BadRequest(new { message = "Airline does not exist." });
            }
            if (!await _context.Aircrafts.AnyAsync(a => a.Id == ticketDto.PlaneId))
            {
                return BadRequest(new { message = "The aircraft does not exist." });
            }
            if (!await _context.Airports.AnyAsync(a => a.Id == ticketDto.DepartureAirportId))
            {
                return BadRequest(new { message = "The departure airport does not exist." });
            }
            if (!await _context.Airports.AnyAsync(a => a.Id == ticketDto.ArrivalAirportId))
            {
                return BadRequest(new { message = "The arrival airport does not exist." });
            }

            if (!ticketDto.DepartureTime.HasValue)
            {
                return BadRequest(new { message = "DepartureTime is required." });
            }
            if (!ticketDto.ArrivalTime.HasValue)
            {
                return BadRequest(new { message = "ArrivalTime is required." });
            }

            DateTime departureTime = ticketDto.DepartureTime.Value;
            DateTime arrivalTime = ticketDto.ArrivalTime.Value;

            if (departureTime >= arrivalTime)
            {
                return BadRequest(new { message = "Departure time must be before the arrival time." });
            }
            if (ticketDto.Price < 0)
            {
                return BadRequest(new { message = "The ticket price cannot be negative." });
            }
            if (ticketDto.AvailableSeats < 0)
            {
                return BadRequest(new { message = "The number of seats cannot be negative." });
            }

            var flightSchedule = new FlightSchedule
            {
                AirlineId = ticketDto.AirlineId,
                DepartureAirportId = ticketDto.DepartureAirportId,
                ArrivalAirportId = ticketDto.ArrivalAirportId,
                AircraftId = ticketDto.PlaneId,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                Stops = ticketDto.Stops,
                Price = ticketDto.Price,
                FlightClass = ticketDto.FlightClass,
                AvailableSeats = ticketDto.AvailableSeats,
                LastUpdate = DateTime.Now,
                DynamicPrice = ticketDto.DynamicPrice
            };

            _context.FlightSchedules.Add(flightSchedule);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetFlightSchedule), new { id = flightSchedule.Id }, flightSchedule);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FlightSchedule>> GetFlightSchedule(int id)
        {
            var flightSchedule = await _context.FlightSchedules
                .Include(fs => fs.Airline)
                .Include(fs => fs.DepartureAirport)
                .Include(fs => fs.ArrivalAirport)
                .Include(fs => fs.Aircraft)
                .FirstOrDefaultAsync(fs => fs.Id == id);
            if (flightSchedule == null || flightSchedule.DepartureTime == null || flightSchedule.ArrivalTime == null) // Thay != null
                return NotFound(new { message = "Flight schedule not found or invalid data" });
            return flightSchedule;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFlightSchedule(int id, TicketDTO ticketDto)
        {
            if (id != ticketDto.Id) return BadRequest(new { message = "ID does not match." });

            var existingFlightSchedule = await _context.FlightSchedules.FindAsync(id);
            if (existingFlightSchedule == null) return NotFound();

            if (!await _context.Airlines.AnyAsync(a => a.Id == ticketDto.AirlineId))
            {
                return BadRequest(new { message = "The airline does not exist." });
            }
            if (!await _context.Aircrafts.AnyAsync(a => a.Id == ticketDto.PlaneId))
            {
                return BadRequest(new { message = "The aircraft does not exist." });
            }
            if (!await _context.Airports.AnyAsync(a => a.Id == ticketDto.DepartureAirportId))
            {
                return BadRequest(new { message = "The departure airport does not exist." });
            }
            if (!await _context.Airports.AnyAsync(a => a.Id == ticketDto.ArrivalAirportId))
            {
                return BadRequest(new { message = "The arrival airport does not exist." });
            }

            if (!ticketDto.DepartureTime.HasValue)
            {
                return BadRequest(new { message = "DepartureTime is required." });
            }
            if (!ticketDto.ArrivalTime.HasValue)
            {
                return BadRequest(new { message = "ArrivalTime is required." });
            }

            DateTime departureTime = ticketDto.DepartureTime.Value;
            DateTime arrivalTime = ticketDto.ArrivalTime.Value;

            if (departureTime >= arrivalTime)
            {
                return BadRequest(new { message = "Departure time must be before the arrival time." });
            }
            if (ticketDto.Price < 0)
            {
                return BadRequest(new { message = "The ticket price cannot be negative." });
            }
            if (ticketDto.AvailableSeats < 0)
            {
                return BadRequest(new { message = "The number of seats cannot be negative." });
            }

            existingFlightSchedule.AirlineId = ticketDto.AirlineId;
            existingFlightSchedule.DepartureAirportId = ticketDto.DepartureAirportId;
            existingFlightSchedule.ArrivalAirportId = ticketDto.ArrivalAirportId;
            existingFlightSchedule.AircraftId = ticketDto.PlaneId;
            existingFlightSchedule.DepartureTime = departureTime;
            existingFlightSchedule.ArrivalTime = arrivalTime;
            existingFlightSchedule.Stops = ticketDto.Stops;
            existingFlightSchedule.Price = ticketDto.Price;
            existingFlightSchedule.FlightClass = ticketDto.FlightClass;
            existingFlightSchedule.AvailableSeats = ticketDto.AvailableSeats;
            existingFlightSchedule.LastUpdate = DateTime.Now;
            existingFlightSchedule.DynamicPrice = ticketDto.DynamicPrice;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlightSchedule(int id)
        {
            var flightSchedule = await _context.FlightSchedules.FindAsync(id);
            if (flightSchedule == null) return NotFound();

            _context.FlightSchedules.Remove(flightSchedule);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("aircrafts/by-airline/{airlineId}")]
        public async Task<ActionResult<IEnumerable<Aircraft>>> GetAircraftsByAirline(int airlineId)
        {
            try
            {
                var aircrafts = await _context.AirlinePlanes
                    .Where(ap => ap.AirlineId == airlineId)
                    .Select(ap => ap.Aircraft)
                    .ToListAsync();

                if (aircrafts == null || !aircrafts.Any())
                {
                    return NotFound(new { message = $"No aircraft found for airlineId {airlineId}." });
                }

                return Ok(aircrafts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }
    }
}