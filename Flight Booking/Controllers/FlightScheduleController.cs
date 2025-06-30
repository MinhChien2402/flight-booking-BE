using Flight_Booking.Data;
using Flight_Booking.DTO;
using Flight_Booking.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.IO;
using System;

namespace Flight_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightScheduleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FlightScheduleController(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Console.WriteLine("FlightScheduleController initialized. Context is valid: " + (_context != null));
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
                    .Where(fs => fs.DepartureTime != null && fs.ArrivalTime != null)
                    .Select(fs => new
                    {
                        id = fs.Id,
                        airline_id = fs.AirlineId,
                        departure_airport_id = fs.DepartureAirportId,
                        arrival_airport_id = fs.ArrivalAirportId,
                        plane_id = fs.AircraftId,
                        departure_time = fs.DepartureTime?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "",
                        arrival_time = fs.ArrivalTime?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "",
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
                Console.WriteLine($"Error in GetFlightSchedules at {DateTime.Now}: {ex.ToString()}");
                return StatusCode(500, new { message = "Error fetching flight schedules", error = ex.Message });
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<object>> SearchTickets([FromBody] TicketSearchDTO searchDto)
        {
            Console.WriteLine($"Nhận tham số tìm kiếm at {DateTime.Now}: " + JsonSerializer.Serialize(searchDto));

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

            searchDto.FlightClass = searchDto.FlightClass.ToLower(); // Chuẩn hóa FlightClass

            var departureDateUtc = searchDto.DepartureDate.Value.ToUniversalTime();
            var localDepartureDate = TimeZoneInfo.ConvertTimeFromUtc(departureDateUtc, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).Date;
            var returnDateUtc = searchDto.ReturnDate.HasValue ? searchDto.ReturnDate.Value.ToUniversalTime() : (DateTime?)null;
            var localReturnDate = returnDateUtc.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(returnDateUtc.Value, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).Date : (DateTime?)null;

            Console.WriteLine($"DepartureDate (Local): {localDepartureDate}, ReturnDate (Local): {localReturnDate}");

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
                              && fs.DepartureTime.Value.Date == localDepartureDate
                              && fs.FlightClass.ToLower() == searchDto.FlightClass
                              && fs.AvailableSeats >= (searchDto.Adults + searchDto.Children))
                    .ToListAsync();

                Console.WriteLine($"Raw outbound tickets count: {outboundTickets.Count}");
                foreach (var ticket in outboundTickets)
                {
                    Console.WriteLine($"Ticket {ticket.Id}: Departure={ticket.DepartureTime}, FlightClass={ticket.FlightClass}, Seats={ticket.AvailableSeats}");
                }

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
                if (searchDto.TripType == "roundtrip" && localReturnDate.HasValue)
                {
                    var returnTicketsQuery = await _context.FlightSchedules
                        .Include(fs => fs.Airline)
                        .Include(fs => fs.DepartureAirport)
                        .Include(fs => fs.ArrivalAirport)
                        .Include(fs => fs.Aircraft)
                        .Where(fs => fs.DepartureAirportId == searchDto.ArrivalAirportId
                                  && fs.ArrivalAirportId == searchDto.DepartureAirportId
                                  && fs.DepartureTime != null
                                  && fs.DepartureTime.Value.Date == localReturnDate.Value
                                  && fs.FlightClass.ToLower() == searchDto.FlightClass
                                  && fs.AvailableSeats >= (searchDto.Adults + searchDto.Children))
                        .ToListAsync();

                    Console.WriteLine($"Raw return tickets count: {returnTicketsQuery.Count}");
                    foreach (var ticket in returnTicketsQuery)
                    {
                        Console.WriteLine($"Ticket {ticket.Id}: Departure={ticket.DepartureTime}, FlightClass={ticket.FlightClass}, Seats={ticket.AvailableSeats}");
                    }

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

                Console.WriteLine($"Tìm thấy vé at {DateTime.Now}: " + JsonSerializer.Serialize(result));
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tìm kiếm vé at {DateTime.Now}: {ex}");
                return StatusCode(500, new { message = "Lỗi server khi tìm kiếm vé. Vui lòng thử lại sau.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<FlightSchedule>> CreateFlightSchedule(TicketDTO ticketDto)
        {
            try
            {
                using var ms = new MemoryStream();
                await Request.Body.CopyToAsync(ms);
                var rawBody = System.Text.Encoding.UTF8.GetString(ms.ToArray());
                Console.WriteLine($"Received Raw Request Body at {DateTime.Now}: {rawBody}");

                Console.WriteLine($"Received TicketDTO at {DateTime.Now}: {JsonSerializer.Serialize(ticketDto)}");
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    Console.WriteLine($"ModelState errors at {DateTime.Now}: {string.Join(", ", errors)}");
                    return BadRequest(new { message = "Invalid model state", errors });
                }

                Console.WriteLine($"Authorization header at {DateTime.Now}: {HttpContext.Request.Headers["Authorization"]}");
                Console.WriteLine($"Checking AirlineId: {ticketDto.AirlineId} (Type: {ticketDto.AirlineId.GetType().Name})");
                var dbAirlines = await _context.Airline.ToListAsync();
                Console.WriteLine($"All Airlines from DB at {DateTime.Now}: " + JsonSerializer.Serialize(dbAirlines));
                var airlineQuery = await _context.Airline.Where(a => a.Id == ticketDto.AirlineId).ToListAsync();
                Console.WriteLine($"Airline query result for Id {ticketDto.AirlineId} at {DateTime.Now}: " + JsonSerializer.Serialize(airlineQuery));
                var airlineExists = await _context.Airline.AnyAsync(a => a.Id == ticketDto.AirlineId);
                Console.WriteLine($"Airline exists: {airlineExists}. Available Airline IDs: " + string.Join(", ", await _context.Airline.Select(a => a.Id).ToListAsync()));
                if (!airlineExists)
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

                Console.WriteLine($"FlightSchedule before save at {DateTime.Now}: " + JsonSerializer.Serialize(flightSchedule));
                _context.FlightSchedules.Add(flightSchedule);
                await _context.SaveChangesAsync();
                Console.WriteLine($"FlightSchedule saved successfully with Id: {flightSchedule.Id} at {DateTime.Now}");
                return CreatedAtAction(nameof(GetFlightSchedule), new { id = flightSchedule.Id }, flightSchedule);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateFlightSchedule at {DateTime.Now}: {ex.ToString()}");
                return StatusCode(500, new { message = "Error creating flight schedule", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FlightSchedule>> GetFlightSchedule(int id)
        {
            try
            {
                var flightSchedule = await _context.FlightSchedules
                    .Include(fs => fs.Airline)
                    .Include(fs => fs.DepartureAirport)
                    .Include(fs => fs.ArrivalAirport)
                    .Include(fs => fs.Aircraft)
                    .FirstOrDefaultAsync(fs => fs.Id == id);
                if (flightSchedule == null || flightSchedule.DepartureTime == null || flightSchedule.ArrivalTime == null)
                    return NotFound(new { message = "Flight schedule not found or invalid data" });
                return flightSchedule;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFlightSchedule at {DateTime.Now}: {ex.ToString()}");
                return StatusCode(500, new { message = "Error fetching flight schedule", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFlightSchedule(int id, TicketDTO ticketDto)
        {
            try
            {
                // Bật bộ đệm cho Request.Body
                Request.EnableBuffering();

                using var ms = new MemoryStream();
                await Request.Body.CopyToAsync(ms);
                var rawBody = System.Text.Encoding.UTF8.GetString(ms.ToArray());
                Console.WriteLine($"Received Raw Request Body for Update at {DateTime.Now}: " + rawBody);

                // Đặt lại vị trí của MemoryStream, không phải Request.Body
                ms.Position = 0;

                // Đọc lại dữ liệu từ MemoryStream nếu cần (tùy chọn)
                using var reader = new StreamReader(ms);
                var bodyContent = await reader.ReadToEndAsync();
                Console.WriteLine($"Body content for processing: {bodyContent}");

                // Đặt lại Request.Body để model binding có thể đọc
                Request.Body.Position = 0; // Lỗi này sẽ không xảy ra nếu dùng EnableBuffering

                Console.WriteLine($"Received TicketDTO for Update at {DateTime.Now}: " + JsonSerializer.Serialize(ticketDto));
                if (id != ticketDto.Id) return BadRequest(new { message = "ID does not match." });

                var existingFlightSchedule = await _context.FlightSchedules.FindAsync(id);
                if (existingFlightSchedule == null) return NotFound();

                Console.WriteLine($"Checking AirlineId for update: {ticketDto.AirlineId} (Type: {ticketDto.AirlineId.GetType().Name})");
                var dbAirlines = await _context.Airline.ToListAsync();
                Console.WriteLine($"All Airlines from DB for update at {DateTime.Now}: " + JsonSerializer.Serialize(dbAirlines));
                var airlineQuery = await _context.Airline.Where(a => a.Id == ticketDto.AirlineId).ToListAsync();
                Console.WriteLine($"Airline query result for update at {DateTime.Now}: " + JsonSerializer.Serialize(airlineQuery));
                var airlineExists = await _context.Airline.AnyAsync(a => a.Id == ticketDto.AirlineId);
                Console.WriteLine($"Airline exists for update: {airlineExists}. Available Airline IDs: " + string.Join(", ", await _context.Airline.Select(a => a.Id).ToListAsync()));
                if (!airlineExists)
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

                Console.WriteLine($"FlightSchedule before update save at {DateTime.Now}: " + JsonSerializer.Serialize(existingFlightSchedule));
                await _context.SaveChangesAsync();
                Console.WriteLine($"FlightSchedule updated successfully with Id: {existingFlightSchedule.Id} at {DateTime.Now}");
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateFlightSchedule at {DateTime.Now}: {ex.ToString()}");
                return StatusCode(500, new { message = "Error updating flight schedule", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlightSchedule(int id)
        {
            try
            {
                var flightSchedule = await _context.FlightSchedules.FindAsync(id);
                if (flightSchedule == null) return NotFound();

                _context.FlightSchedules.Remove(flightSchedule);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteFlightSchedule at {DateTime.Now}: {ex.ToString()}");
                return StatusCode(500, new { message = "Error deleting flight schedule", error = ex.Message });
            }
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

                return aircrafts.Any() ? Ok(aircrafts) : Ok(new List<Aircraft>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAircraftsByAirline at {DateTime.Now}: {ex.ToString()}");
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }
    }
}