using Flight_Booking.Data;
using Flight_Booking.DTO;
using Flight_Booking.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Flight_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingDTO request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                if (request == null || request.Passengers == null || !request.Passengers.Any())
                {
                    return BadRequest(new { message = "Booking request and passenger information are required" });
                }

                // Kiểm tra các trường bắt buộc và ngày sinh hợp lệ
                foreach (var passenger in request.Passengers)
                {
                    if (string.IsNullOrEmpty(passenger.Title) ||
                        string.IsNullOrEmpty(passenger.FirstName) ||
                        string.IsNullOrEmpty(passenger.LastName) ||
                        string.IsNullOrEmpty(passenger.PassportNumber) ||
                        passenger.DateOfBirth == default ||
                        passenger.PassportExpiry == default)
                    {
                        return BadRequest(new { message = "All passenger fields are required" });
                    }
                    if (passenger.DateOfBirth > DateTime.Now)
                    {
                        return BadRequest(new { message = "Date of birth not in the future" });
                    }
                }

                // Kiểm tra vé có tồn tại và có ghế trống không
                var ticket = await _context.Tickets.FindAsync(request.TicketId);
                if (ticket == null)
                {
                    return NotFound(new { message = "Ticket not found" });
                }
                if (ticket.AvailableSeats < 1)
                {
                    return BadRequest(new { message = "No available seats for this flight" });
                }

                // Kiểm tra UserId tồn tại
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return BadRequest(new { message = $"User with UserId {userId} not found" });
                }

                var booking = new Booking
                {
                    UserId = userId,
                    TicketId = request.TicketId,
                    TotalPrice = request.TotalPrice,
                    BookingDate = DateTime.Now,
                    Status = "Confirmed",
                    Passengers = request.Passengers.Select(p => new Passenger
                    {
                        Title = p.Title,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        DateOfBirth = p.DateOfBirth,
                        PassportNumber = p.PassportNumber,
                        PassportExpiry = p.PassportExpiry
                    }).ToList()
                };

                // Giảm số ghế trống
                ticket.AvailableSeats -= 1;
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Booking confirmed successfully", bookingId = booking.BookingId });
            }
            catch (DbUpdateException ex)
            {
                var errorDetails = ex.InnerException != null ? ex.InnerException.ToString() : ex.Message;
                Console.WriteLine($"Lỗi DbUpdateException: {errorDetails}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Lỗi khi lưu dữ liệu", error = errorDetails });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi chung: {ex.ToString()}");
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUserBookings()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var bookings = await _context.Bookings
                    .Where(b => b.UserId == userId && b.Status == "Confirmed")
                    .Include(b => b.Ticket)
                        .ThenInclude(t => t.Airline)
                    .Include(b => b.Ticket)
                        .ThenInclude(t => t.DepartureAirport)
                    .Include(b => b.Ticket)
                        .ThenInclude(t => t.ArrivalAirport)
                    .Select(b => new
                    {
                        BookingId = b.BookingId,
                        TicketId = b.TicketId,
                        Airline = b.Ticket.Airline.Name,
                        From = b.Ticket.DepartureAirport.Name,
                        To = b.Ticket.ArrivalAirport.Name,
                        Departure = b.Ticket.DepartureTime.ToString("dd/MM/yyyy HH:mm"),
                        Arrival = b.Ticket.ArrivalTime.ToString("dd/MM/yyyy HH:mm"),
                        Duration = (b.Ticket.ArrivalTime - b.Ticket.DepartureTime).TotalHours.ToString("F1") + "h",
                        BookedOn = b.BookingDate.ToString("dd/MM/yyyy HH:mm"),
                        TotalPrice = b.TotalPrice
                    })
                    .ToListAsync();

                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBookingDetail(int bookingId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var booking = await _context.Bookings
                    .Where(b => b.BookingId == bookingId && b.UserId == userId)
                    .Include(b => b.Ticket)
                        .ThenInclude(t => t.Airline)
                    .Include(b => b.Ticket)
                        .ThenInclude(t => t.DepartureAirport)
                    .Include(b => b.Ticket)
                        .ThenInclude(t => t.ArrivalAirport)
                    .Include(b => b.Passengers)
                    .Select(b => new
                    {
                        BookingId = b.BookingId,
                        TicketId = b.TicketId,
                        Airline = b.Ticket.Airline.Name,
                        From = b.Ticket.DepartureAirport.Name,
                        To = b.Ticket.ArrivalAirport.Name,
                        Departure = b.Ticket.DepartureTime.ToString("dd/MM/yyyy HH:mm"),
                        Arrival = b.Ticket.ArrivalTime.ToString("dd/MM/yyyy HH:mm"),
                        Duration = (b.Ticket.ArrivalTime - b.Ticket.DepartureTime).TotalHours.ToString("F1") + "h",
                        BookedOn = b.BookingDate.ToString("dd/MM/yyyy HH:mm"),
                        TotalPrice = b.TotalPrice,
                        Status = b.Status,
                        Passengers = b.Passengers.Select(p => new
                        {
                            Title = p.Title,
                            FirstName = p.FirstName,
                            LastName = p.LastName,
                            DateOfBirth = p.DateOfBirth.ToString("dd/MM/yyyy"),
                            PassportNumber = p.PassportNumber,
                            PassportExpiry = p.PassportExpiry.ToString("dd/MM/yyyy")
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (booking == null)
                {
                    return NotFound(new { message = "Không tìm thấy booking hoặc bạn không có quyền truy cập" });
                }

                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }
    }
}