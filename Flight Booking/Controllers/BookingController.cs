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
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

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

                if (request == null || request.Passengers == null || !request.Passengers.Any() || !request.OutboundTicketId.HasValue)
                {
                    return BadRequest(new { message = "Booking request, passenger information, and outbound ticket ID are required" });
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

                // Kiểm tra vé outbound
                var outboundTicket = await _context.Tickets.FindAsync(request.OutboundTicketId.Value);
                if (outboundTicket == null || outboundTicket.AvailableSeats < 1)
                {
                    return BadRequest(new { message = "Outbound ticket not available" });
                }

                // Kiểm tra vé return (nếu có)
                Tickets returnTicket = null;
                if (request.ReturnTicketId.HasValue)
                {
                    returnTicket = await _context.Tickets.FindAsync(request.ReturnTicketId.Value);
                    if (returnTicket == null || returnTicket.AvailableSeats < 1)
                    {
                        return BadRequest(new { message = "Return ticket not available" });
                    }
                }

                // Kiểm tra UserId tồn tại
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return BadRequest(new { message = $"User with UserId {userId} not found" });
                }

                // Tạo booking mới và các BookingTickets
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    var booking = new Booking
                    {
                        UserId = userId,
                        TotalPrice = (decimal)request.TotalPrice,
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

                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync(); // Lưu để sinh BookingId

                    // Thêm BookingTicket cho outbound
                    var outboundBookingTicket = new BookingTicket
                    {
                        BookingId = booking.BookingId,
                        TicketId = request.OutboundTicketId.Value
                    };
                    _context.BookingTickets.Add(outboundBookingTicket); // Thêm trực tiếp vào context
                    outboundTicket.AvailableSeats -= 1;

                    // Thêm BookingTicket cho return (nếu có)
                    if (request.ReturnTicketId.HasValue)
                    {
                        var returnBookingTicket = new BookingTicket
                        {
                            BookingId = booking.BookingId,
                            TicketId = request.ReturnTicketId.Value
                        };
                        _context.BookingTickets.Add(returnBookingTicket); // Thêm trực tiếp vào context
                        returnTicket.AvailableSeats -= 1;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Console.WriteLine($"Created booking with ID {booking.BookingId} for user {userId}");
                    return Ok(new { message = "Booking confirmed successfully", bookingId = booking.BookingId });
                }
            }
            catch (DbUpdateException ex)
            {
                var errorDetails = ex.InnerException != null ? ex.InnerException.ToString() : ex.Message;
                Console.WriteLine($"Lỗi DbUpdateException: {errorDetails}");
                return StatusCode(500, new { message = "Lỗi khi lưu dữ liệu", error = errorDetails });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi chung: {ex.ToString()}");
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
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
                    .Include(b => b.BookingTickets)
                        .ThenInclude(bt => bt.Ticket)
                        .ThenInclude(t => t.Airline)
                    .Include(b => b.BookingTickets)
                        .ThenInclude(bt => bt.Ticket)
                        .ThenInclude(t => t.DepartureAirport)
                    .Include(b => b.BookingTickets)
                        .ThenInclude(bt => bt.Ticket)
                        .ThenInclude(t => t.ArrivalAirport)
                    .AsSplitQuery() // Tối ưu hóa việc tải dữ liệu
                    .Select(b => new
                    {
                        bookingId = b.BookingId,
                        tickets = b.BookingTickets
                            .Where(bt => bt.Ticket != null) // Loại bỏ các bản ghi không hợp lệ
                            .Select(bt => new
                            {
                                airline = bt.Ticket.Airline,
                                departureAirport = bt.Ticket.DepartureAirport,
                                arrivalAirport = bt.Ticket.ArrivalAirport,
                                departureTime = bt.Ticket.DepartureTime,
                                arrivalTime = bt.Ticket.ArrivalTime,
                                availableSeats = bt.Ticket.AvailableSeats,
                                flightClass = bt.Ticket.FlightClass,
                                id = bt.Ticket.Id,
                                plane = bt.Ticket.Plane,
                                price = bt.Ticket.Price
                            })
                            .ToList(),
                        bookedOn = b.BookingDate.ToString("dd/MM/yyyy HH:mm"),
                        totalPrice = b.TotalPrice
                    })
                    .ToListAsync();

                // Kiểm tra và log dữ liệu
                if (bookings.Any(b => !b.tickets.Any()))
                {
                    Console.WriteLine($"Warning: Some bookings have empty tickets for user {userId}");
                }
                Console.WriteLine("Retrieved Bookings: " + Newtonsoft.Json.JsonConvert.SerializeObject(bookings));

                return Ok(bookings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserBookings: {ex.Message}");
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
                    .Where(b => b.BookingId == bookingId && b.UserId == userId && b.Status == "Confirmed")
                    .Include(b => b.BookingTickets)
                        .ThenInclude(bt => bt.Ticket)
                        .ThenInclude(t => t.Airline)
                    .Include(b => b.BookingTickets)
                        .ThenInclude(bt => bt.Ticket)
                        .ThenInclude(t => t.DepartureAirport)
                    .Include(b => b.BookingTickets)
                        .ThenInclude(bt => bt.Ticket)
                        .ThenInclude(t => t.ArrivalAirport)
                    .Include(b => b.Passengers)
                    .Select(b => new
                    {
                        BookingId = b.BookingId,
                        Tickets = b.BookingTickets
                            .Where(bt => bt.Ticket != null)
                            .Select(bt => new
                            {
                                Airline = bt.Ticket.Airline.Name,
                                From = bt.Ticket.DepartureAirport.Name,
                                To = bt.Ticket.ArrivalAirport.Name,
                                Departure = bt.Ticket.DepartureTime.ToString("dd/MM/yyyy HH:mm"),
                                Arrival = bt.Ticket.ArrivalTime.ToString("dd/MM/yyyy HH:mm"),
                                Duration = (bt.Ticket.ArrivalTime - bt.Ticket.DepartureTime).TotalHours.ToString("F1") + "h",
                                FlightClass = bt.Ticket.FlightClass,
                                Price = bt.Ticket.Price
                            })
                            .ToList(),
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

                Console.WriteLine($"Retrieved booking detail for BookingId {bookingId}: " + Newtonsoft.Json.JsonConvert.SerializeObject(booking));
                return Ok(booking);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetBookingDetail: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{bookingId}/pdf")]
        public async Task<IActionResult> GenerateBookingPass(int bookingId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { message = "Invalid token" });

                var booking = await _context.Bookings
                    .Where(b => b.BookingId == bookingId && b.UserId == userId)
                    .Include(b => b.BookingTickets)
                        .ThenInclude(bt => bt.Ticket)
                        .ThenInclude(t => t.Airline)
                    .Include(b => b.BookingTickets)
                        .ThenInclude(bt => bt.Ticket)
                        .ThenInclude(t => t.DepartureAirport)
                    .Include(b => b.BookingTickets)
                        .ThenInclude(bt => bt.Ticket)
                        .ThenInclude(t => t.ArrivalAirport)
                    .FirstOrDefaultAsync();

                if (booking == null)
                    return NotFound(new { message = "Booking not found or you do not have access" });

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return BadRequest(new { message = "User not found for the given token" });

                var passengerName = user.FullName ?? "Unknown";

                using (var memoryStream = new MemoryStream())
                {
                    var writer = new PdfWriter(memoryStream);
                    var pdf = new PdfDocument(writer);
                    var document = new Document(pdf, PageSize.A4);
                    document.SetMargins(40, 40, 40, 40);

                    var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                    // Title
                    document.Add(new Paragraph("E-Ticket")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(boldFont)
                        .SetFontSize(20)
                        .SetMarginBottom(10));

                    // Process all tickets
                    foreach (var ticket in booking.BookingTickets.Select(bt => bt.Ticket))
                    {
                        document.Add(new Paragraph($"{ticket.DepartureAirport.Name} to {ticket.ArrivalAirport.Name}")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFont(font)
                            .SetFontSize(12));

                        document.Add(new Paragraph($"{ticket.Airline.Name}")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFont(boldFont)
                            .SetFontSize(14)
                            .SetMarginBottom(20));

                        var table = new Table(new float[] { 2f, 2f, 2f }).UseAllAvailableWidth();
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Flight").SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("From").SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("To").SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));

                        table.AddCell(new Cell().Add(new Paragraph(ticket.Airline.Name).SetFont(font)));
                        table.AddCell(new Cell().Add(new Paragraph(ticket.DepartureAirport.Name).SetFont(font)));
                        table.AddCell(new Cell().Add(new Paragraph(ticket.ArrivalAirport.Name).SetFont(font)));

                        table.AddCell(new Cell().Add(new Paragraph("Date").SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                        table.AddCell(new Cell().Add(new Paragraph(ticket.DepartureTime.ToString("dd/MM/yyyy")).SetFont(font)));
                        table.AddCell(new Cell().Add(new Paragraph(ticket.ArrivalTime.ToString("dd/MM/yyyy")).SetFont(font)));

                        table.AddCell(new Cell().Add(new Paragraph("Time").SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                        table.AddCell(new Cell().Add(new Paragraph(ticket.DepartureTime.ToString("HH:mm")).SetFont(font)));
                        table.AddCell(new Cell().Add(new Paragraph(ticket.ArrivalTime.ToString("HH:mm")).SetFont(font)));

                        table.AddCell(new Cell().Add(new Paragraph("Duration").SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                        table.AddCell(new Cell().Add(new Paragraph((ticket.ArrivalTime - ticket.DepartureTime).TotalHours.ToString("F1") + " hrs").SetFont(font)));
                        table.AddCell(new Cell());

                        document.Add(table);
                        document.Add(new Paragraph("\n")); // Thêm dòng trống giữa các vé
                    }

                    // Passenger & Price
                    document.Add(new Paragraph($"\nPassenger: {passengerName}")
                        .SetFont(font)
                        .SetFontSize(12));

                    document.Add(new Paragraph($"Total Price: ${booking.TotalPrice:F2}")
                        .SetFont(font)
                        .SetFontSize(12)
                        .SetMarginBottom(20));

                    // Ticket Code
                    document.Add(new Paragraph($"Ticket Code: {booking.BookingId}")
                        .SetFont(boldFont)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(14));

                    document.Close();
                    return File(memoryStream.ToArray(), "application/pdf", $"booking_{booking.BookingId}.pdf");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GenerateBookingPass: {ex.Message}");
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }
    }
}