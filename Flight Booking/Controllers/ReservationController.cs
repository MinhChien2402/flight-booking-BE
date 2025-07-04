﻿using Flight_Booking.Data;
using Flight_Booking.DTO;
using Flight_Booking.Model;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Flight_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservationController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] BookingDTO request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (request == null || request.Passengers == null || !request.Passengers.Any() || !request.OutboundTicketId.HasValue)
                {
                    return BadRequest(new { message = "Reservation request, passenger information, and outbound ticket ID are required" });
                }

                foreach (var passenger in request.Passengers)
                {
                    if (string.IsNullOrEmpty(passenger.Title) ||
                        string.IsNullOrEmpty(passenger.FirstName) ||
                        string.IsNullOrEmpty(passenger.LastName) ||
                        string.IsNullOrEmpty(passenger.PassportNumber) ||
                        passenger.DateOfBirth == default(DateTime) ||
                        passenger.PassportExpiry == default(DateTime))
                    {
                        return BadRequest(new { message = "All passenger fields are required" });
                    }
                    if (passenger.DateOfBirth > DateTime.Now)
                    {
                        return BadRequest(new { message = "Date of birth cannot be in the future" });
                    }
                }

                var outboundFlightSchedule = await _context.FlightSchedules.FindAsync(request.OutboundTicketId.Value);
                if (outboundFlightSchedule == null || outboundFlightSchedule.AvailableSeats < 1)
                {
                    return BadRequest(new { message = "Outbound flight schedule not available" });
                }

                FlightSchedule returnFlightSchedule = null;
                if (request.ReturnTicketId.HasValue)
                {
                    returnFlightSchedule = await _context.FlightSchedules.FindAsync(request.ReturnTicketId.Value);
                    if (returnFlightSchedule == null || returnFlightSchedule.AvailableSeats < 1)
                    {
                        return BadRequest(new { message = "Return flight schedule not available" });
                    }
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return BadRequest(new { message = $"User with ID {userId} not found" });
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    var reservation = new Reservation
                    {
                        UserId = userId,
                        TotalFare = (decimal)request.TotalPrice,
                        ReservationDate = DateTime.Now,
                        ReservationStatus = "Confirmed",
                        ConfirmationNumber = Guid.NewGuid().ToString(),
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

                    _context.Reservations.Add(reservation);
                    await _context.SaveChangesAsync();

                    var outboundReservationTicket = new ReservationTicket
                    {
                        ReservationId = reservation.Id,
                        FlightScheduleId = request.OutboundTicketId.Value
                    };
                    _context.ReservationTickets.Add(outboundReservationTicket);
                    outboundFlightSchedule.AvailableSeats -= 1;

                    if (request.ReturnTicketId.HasValue)
                    {
                        var returnReservationTicket = new ReservationTicket
                        {
                            ReservationId = reservation.Id,
                            FlightScheduleId = request.ReturnTicketId.Value
                        };
                        _context.ReservationTickets.Add(returnReservationTicket);
                        returnFlightSchedule.AvailableSeats -= 1;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Console.WriteLine($"Created reservation with ID {reservation.Id} for user {userId}");
                    return Ok(new { message = "Reservation confirmed successfully", reservationId = reservation.Id });
                }
            }
            catch (DbUpdateException ex)
            {
                var errorDetails = ex.InnerException?.ToString() ?? ex.Message;
                Console.WriteLine($"DbUpdateException: {errorDetails}");
                return StatusCode(500, new { message = "Error saving data", error = errorDetails });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.ToString()}");
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("user")]
        public async Task<IActionResult> GetUserReservations()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"Raw UserId claim from token: {userIdClaim}");

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    Console.WriteLine("No valid UserId found in token");
                    return Unauthorized(new { message = "No valid UserId in token" });
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    Console.WriteLine($"Invalid UserId format: {userIdClaim}, attempting to use as is");
                    if (int.TryParse(userIdClaim, NumberStyles.Integer, CultureInfo.InvariantCulture, out userId))
                    {
                        Console.WriteLine($"Parsed UserId: {userId}");
                    }
                    else
                    {
                        return Unauthorized(new { message = "Invalid UserId format in token" });
                    }
                }

                Console.WriteLine($"Extracted UserId from token: {userId}");

                var reservations = await _context.Reservations
                    .Where(r => r.UserId == userId && (r.ReservationStatus == "Confirmed" || r.ReservationStatus == "Blocked"))
                    .Include(r => r.ReservationTickets)
                        .ThenInclude(rt => rt.FlightSchedule)
                        .ThenInclude(fs => fs.Airline)
                    .Include(r => r.ReservationTickets)
                        .ThenInclude(rt => rt.FlightSchedule)
                        .ThenInclude(fs => fs.DepartureAirport)
                    .Include(r => r.ReservationTickets)
                        .ThenInclude(rt => rt.FlightSchedule)
                        .ThenInclude(fs => fs.ArrivalAirport)
                    .ToListAsync();

                Console.WriteLine($"Total reservations found: {reservations.Count}");

                var result = reservations.Select(r => new
                {
                    reservationId = r.Id,
                    tickets = r.ReservationTickets
                        .Where(rt => rt.FlightSchedule != null)
                        .Select(rt => new
                        {
                            airline = rt.FlightSchedule.Airline != null ? new { Name = rt.FlightSchedule.Airline.Name } : null,
                            departureAirport = rt.FlightSchedule.DepartureAirport != null ? new { Name = rt.FlightSchedule.DepartureAirport.Name } : null,
                            arrivalAirport = rt.FlightSchedule.ArrivalAirport != null ? new { Name = rt.FlightSchedule.ArrivalAirport.Name } : null,
                            departureTime = rt.FlightSchedule.DepartureTime.HasValue ? rt.FlightSchedule.DepartureTime.Value.ToString("dd/MM/yyyy HH:mm") : null,
                            arrivalTime = rt.FlightSchedule.ArrivalTime.HasValue ? rt.FlightSchedule.ArrivalTime.Value.ToString("dd/MM/yyyy HH:mm") : null,
                            id = rt.FlightSchedule.Id,
                            price = rt.FlightSchedule.Price
                        })
                        .ToList(),
                    bookedOn = r.ReservationDate.ToString("dd/MM/yyyy HH:mm"),
                    totalPrice = r.TotalFare,
                    status = r.ReservationStatus,
                    confirmationNumber = r.ConfirmationNumber
                }).ToList();

                Console.WriteLine($"Serialized result: {Newtonsoft.Json.JsonConvert.SerializeObject(result)}");

                if (!result.Any())
                {
                    return NotFound(new { message = $"No reservations found for UserId: {userId}" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserReservations: {ex.Message}");
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{reservationId}")]
        public async Task<IActionResult> GetReservationDetail(int reservationId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var reservation = await _context.Reservations
                    .Where(r => r.Id == reservationId && r.UserId == userId && (r.ReservationStatus == "Confirmed" || r.ReservationStatus == "Blocked"))
                    .Include(r => r.ReservationTickets)
                        .ThenInclude(rt => rt.FlightSchedule)
                        .ThenInclude(fs => fs.Airline)
                    .Include(r => r.ReservationTickets)
                        .ThenInclude(rt => rt.FlightSchedule)
                        .ThenInclude(fs => fs.DepartureAirport)
                    .Include(r => r.ReservationTickets)
                        .ThenInclude(rt => rt.FlightSchedule)
                        .ThenInclude(fs => fs.ArrivalAirport)
                    .Include(r => r.Passengers)
                    .Select(r => new
                    {
                        ReservationId = r.Id,
                        Tickets = r.ReservationTickets
                            .Where(rt => rt.FlightSchedule != null)
                            .Select(rt => new
                            {
                                Airline = rt.FlightSchedule.Airline.Name,
                                From = rt.FlightSchedule.DepartureAirport.Name,
                                To = rt.FlightSchedule.ArrivalAirport.Name,
                                Departure = rt.FlightSchedule.DepartureTime.HasValue ? rt.FlightSchedule.DepartureTime.Value.ToString("dd/MM/yyyy HH:mm") : "N/A",
                                Arrival = rt.FlightSchedule.ArrivalTime.HasValue ? rt.FlightSchedule.ArrivalTime.Value.ToString("dd/MM/yyyy HH:mm") : "N/A",
                                Duration = rt.FlightSchedule.ArrivalTime.HasValue && rt.FlightSchedule.DepartureTime.HasValue
                                    ? (rt.FlightSchedule.ArrivalTime.Value - rt.FlightSchedule.DepartureTime.Value).TotalHours.ToString("F1") + "h"
                                    : "N/A",
                                FlightClass = rt.FlightSchedule.FlightClass,
                                Price = rt.FlightSchedule.Price
                            })
                            .ToList(),
                        BookedOn = r.ReservationDate.ToString("dd/MM/yyyy HH:mm"),
                        TotalPrice = r.TotalFare,
                        Status = r.ReservationStatus,
                        ConfirmationNumber = r.ConfirmationNumber,
                        Passengers = r.Passengers.Select(p => new
                        {
                            Title = p.Title,
                            FirstName = p.FirstName,
                            LastName = p.LastName,
                            DateOfBirth = p.DateOfBirth.HasValue ? p.DateOfBirth.Value.ToString("dd/MM/yyyy") : "N/A",
                            PassportNumber = p.PassportNumber,
                            PassportExpiry = p.PassportExpiry.HasValue ? p.PassportExpiry.Value.ToString("dd/MM/yyyy") : "N/A"
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (reservation == null)
                {
                    return NotFound(new { message = "Reservation not found or you do not have access" });
                }

                Console.WriteLine($"Retrieved reservation detail for ReservationId {reservationId}: " + Newtonsoft.Json.JsonConvert.SerializeObject(reservation));
                return Ok(reservation);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetReservationDetail: {ex.Message}");
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{reservationId}/pdf")]
        public async Task<IActionResult> GenerateReservationPass(int reservationId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (userId == 0)
                    return Unauthorized(new { message = "Invalid token" });

                var reservation = await _context.Reservations
                    .Where(r => r.Id == reservationId && r.UserId == userId)
                    .Include(r => r.ReservationTickets)
                        .ThenInclude(rt => rt.FlightSchedule)
                        .ThenInclude(fs => fs.Airline)
                    .Include(r => r.ReservationTickets)
                        .ThenInclude(rt => rt.FlightSchedule)
                        .ThenInclude(fs => fs.DepartureAirport)
                    .Include(r => r.ReservationTickets)
                        .ThenInclude(rt => rt.FlightSchedule)
                        .ThenInclude(fs => fs.ArrivalAirport)
                    .FirstOrDefaultAsync();

                if (reservation == null)
                    return NotFound(new { message = "Reservation not found or you do not have access" });

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

                    document.Add(new Paragraph("E-Ticket")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(boldFont)
                        .SetFontSize(20)
                        .SetMarginBottom(10));

                    foreach (var flightSchedule in reservation.ReservationTickets.Select(rt => rt.FlightSchedule))
                    {
                        document.Add(new Paragraph($"{flightSchedule.DepartureAirport.Name} to {flightSchedule.ArrivalAirport.Name}")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFont(font)
                            .SetFontSize(12));

                        document.Add(new Paragraph($"{flightSchedule.Airline.Name}")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFont(boldFont)
                            .SetFontSize(14)
                            .SetMarginBottom(20));

                        var table = new Table(new float[] { 2f, 2f, 2f }).UseAllAvailableWidth();
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Flight").SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("From").SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("To").SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));

                        table.AddCell(new Cell().Add(new Paragraph(flightSchedule.Airline.Name).SetFont(font)));
                        table.AddCell(new Cell().Add(new Paragraph(flightSchedule.DepartureAirport.Name).SetFont(font)));
                        table.AddCell(new Cell().Add(new Paragraph(flightSchedule.ArrivalAirport.Name).SetFont(font)));

                        table.AddCell(new Cell().Add(new Paragraph("Date").SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                        table.AddCell(new Cell().Add(new Paragraph(flightSchedule.DepartureTime.HasValue ? flightSchedule.DepartureTime.Value.ToString("dd/MM/yyyy") : "N/A").SetFont(font)));
                        table.AddCell(new Cell().Add(new Paragraph(flightSchedule.ArrivalTime.HasValue ? flightSchedule.ArrivalTime.Value.ToString("dd/MM/yyyy") : "N/A").SetFont(font)));

                        table.AddCell(new Cell().Add(new Paragraph("Time").SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                        table.AddCell(new Cell().Add(new Paragraph(flightSchedule.DepartureTime.HasValue ? flightSchedule.DepartureTime.Value.ToString("HH:mm") : "N/A").SetFont(font)));
                        table.AddCell(new Cell().Add(new Paragraph(flightSchedule.ArrivalTime.HasValue ? flightSchedule.ArrivalTime.Value.ToString("HH:mm") : "N/A").SetFont(font)));

                        table.AddCell(new Cell().Add(new Paragraph("Duration").SetFont(boldFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                        table.AddCell(new Cell().Add(new Paragraph(flightSchedule.ArrivalTime.HasValue && flightSchedule.DepartureTime.HasValue
                            ? (flightSchedule.ArrivalTime.Value - flightSchedule.DepartureTime.Value).TotalHours.ToString("F1") + " hrs"
                            : "N/A").SetFont(font)));
                        table.AddCell(new Cell());

                        document.Add(table);
                        document.Add(new Paragraph("\n"));
                    }

                    document.Add(new Paragraph($"\nPassenger: {passengerName}")
                        .SetFont(font)
                        .SetFontSize(12));

                    document.Add(new Paragraph($"Total Price: ${reservation.TotalFare:F2}")
                        .SetFont(font)
                        .SetFontSize(12)
                        .SetMarginBottom(20));

                    document.Add(new Paragraph($"Confirmation Number: {reservation.ConfirmationNumber}")
                        .SetFont(boldFont)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(14));

                    document.Close();
                    return File(memoryStream.ToArray(), "application/pdf", $"reservation_{reservation.Id}.pdf");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GenerateReservationPass: {ex.Message}");
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("block")]
        public async Task<IActionResult> BlockReservation([FromBody] BlockTicketDTO dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var flightSchedule = await _context.FlightSchedules
                    .Include(fs => fs.DepartureAirport)
                    .Include(fs => fs.ArrivalAirport)
                    .FirstOrDefaultAsync(fs => fs.Id == dto.FlightScheduleId);

                if (flightSchedule == null || flightSchedule.AvailableSeats < 1 || !flightSchedule.DepartureTime.HasValue)
                    return BadRequest(new { message = "Flight schedule not available" });

                var daysDiff = (flightSchedule.DepartureTime.Value - DateTime.Now).TotalDays;
                if (daysDiff < 14)
                    return BadRequest(new { message = "Cannot block ticket, departure is within 2 weeks. Please buy instead." });

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return BadRequest(new { message = "User not found" });

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    var reservation = new Reservation
                    {
                        UserId = userId,
                        FlightScheduleId = dto.FlightScheduleId,
                        BlockExpiryDate = DateTime.Now.AddDays(14),
                        ReservationStatus = "Blocked",
                        TotalFare = flightSchedule.Price,
                        ConfirmationNumber = Guid.NewGuid().ToString(),
                        CancellationRules = "Default: 90% refund if cancelled > 7 days, 50% if < 7 days",
                        ReservationDate = DateTime.Now
                    };

                    var distance = flightSchedule.Distance ?? 1000;
                    user.SkyMiles += (decimal)(distance * 0.1);

                    _context.Reservations.Add(reservation);
                    await _context.SaveChangesAsync();

                    // Thêm ReservationTicket
                    var reservationTicket = new ReservationTicket
                    {
                        ReservationId = reservation.Id,
                        FlightScheduleId = dto.FlightScheduleId
                    };
                    _context.ReservationTickets.Add(reservationTicket);
                    flightSchedule.AvailableSeats -= 1;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { message = "Reservation blocked", reservationId = reservation.Id, confirmationNumber = reservation.ConfirmationNumber });
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"DbUpdateException: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(500, new { message = "Server error", error = ex.InnerException?.Message ?? ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("confirm/{reservationId}")]
        public async Task<IActionResult> ConfirmReservation(int reservationId)
        {
            try
            {
                // Lấy userId từ token
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Tìm reservation với reservationId và userId
                var reservation = await _context.Reservations
                    .Include(r => r.ReservationTickets)
                        .ThenInclude(rt => rt.FlightSchedule)
                    .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);

                // Kiểm tra reservation hợp lệ và trạng thái
                if (reservation == null || reservation.ReservationStatus != "Blocked")
                    return BadRequest(new { message = "Invalid or non-blocked reservation" });

                // Kiểm tra flight schedule
                var flightSchedule = reservation.ReservationTickets.FirstOrDefault()?.FlightSchedule;
                if (flightSchedule == null || !flightSchedule.DepartureTime.HasValue)
                    return BadRequest(new { message = "Invalid flight schedule" });

                // Kiểm tra thời hạn xác nhận (trước 2 tuần)
                var daysDiff = (flightSchedule.DepartureTime.Value - DateTime.Now).TotalDays;
                if (daysDiff < 14)
                    return BadRequest(new { message = "Cannot confirm: Departure is within 2 weeks" });

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    // Cập nhật trạng thái và số xác nhận
                    reservation.ReservationStatus = "Confirmed";
                    reservation.ConfirmationNumber = Guid.NewGuid().ToString();
                    reservation.BlockExpiryDate = null; // Xóa thời hạn giữ chỗ

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        message = "Reservation confirmed successfully",
                        reservationId = reservation.Id,
                        confirmationNumber = reservation.ConfirmationNumber
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ConfirmReservation: {ex.Message}");
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("reschedule/{reservationId}")]
        public async Task<IActionResult> RescheduleReservation(int reservationId, [FromBody] RescheduleDTO dto)
        {
            var reservation = await _context.Reservations.Include(r => r.FlightSchedule).FirstAsync(r => r.Id == reservationId);
            var newFlightSchedule = await _context.FlightSchedules.FindAsync(dto.NewFlightScheduleId);
            if (newFlightSchedule == null || newFlightSchedule.AvailableSeats < 1 || !newFlightSchedule.DepartureTime.HasValue)
                return BadRequest(new { message = "New flight schedule not available" });

            var history = new ReservationHistory
            {
                ReservationId = reservationId,
                ActionType = "Reschedule",
                OldDate = reservation.FlightSchedule.DepartureTime,
                NewDate = newFlightSchedule.DepartureTime,
                RefundAmount = reservation.TotalFare - newFlightSchedule.Price
            };
            reservation.FlightScheduleId = dto.NewFlightScheduleId;
            reservation.TotalFare = newFlightSchedule.Price;
            _context.ReservationHistories.Add(history);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Reservation rescheduled" });
        }

        [Authorize]
        [HttpDelete("cancel/{reservationId}")]
        public async Task<IActionResult> CancelReservation(int reservationId)
        {
            var reservation = await _context.Reservations.Include(r => r.FlightSchedule).FirstAsync(r => r.Id == reservationId);
            if (reservation.FlightSchedule == null || !reservation.FlightSchedule.DepartureTime.HasValue)
            {
                return BadRequest(new { message = "Invalid flight schedule data" });
            }

            var timeSpan = reservation.FlightSchedule.DepartureTime.Value - DateTime.Now; // Trả về TimeSpan
            int daysDiff = (int)timeSpan.TotalDays; // Không cần HasValue vì timeSpan là TimeSpan

            var refund = daysDiff > 7 ? reservation.TotalFare * 0.9m : daysDiff > 0 ? reservation.TotalFare * 0.5m : 0;
            var history = new ReservationHistory
            {
                ReservationId = reservationId,
                ActionType = "Cancel",
                OldDate = reservation.FlightSchedule.DepartureTime,
                RefundAmount = refund
            };
            reservation.ReservationStatus = "Cancelled";
            _context.ReservationHistories.Add(history);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Reservation cancelled", refund = history.RefundAmount });
        }

        [Authorize]
        [HttpPost("cancel-expired")]
        public async Task<IActionResult> CancelExpiredReservations()
        {
            try
            {
                var currentDate = DateTime.Now;

                var expiredReservations = await _context.Reservations
                    .Where(r => r.ReservationStatus == "Blocked" &&
                               (r.BlockExpiryDate < currentDate || // Hết hạn 2 tuần từ ngày giữ chỗ
                                (r.FlightScheduleId.HasValue && // Kiểm tra DepartureTime
                                 _context.FlightSchedules.Any(fs => fs.Id == r.FlightScheduleId && fs.DepartureTime < currentDate.AddDays(14)))))
                    .ToListAsync();

                if (!expiredReservations.Any())
                    return Ok(new { message = "No expired reservations found" });

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    foreach (var reservation in expiredReservations)
                    {
                        reservation.ReservationStatus = "Cancelled";
                        reservation.BlockExpiryDate = null; // Làm sạch dữ liệu

                        // Tăng lại số ghế trống trong FlightSchedule
                        var flightSchedule = await _context.FlightSchedules.FindAsync(reservation.FlightScheduleId);
                        if (flightSchedule != null)
                        {
                            flightSchedule.AvailableSeats += 1;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Console.WriteLine($"Cancelled {expiredReservations.Count} expired reservations at {currentDate}");
                    return Ok(new { message = $"Cancelled {expiredReservations.Count} expired reservations" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CancelExpiredReservations: {ex.Message}");
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }
    }
}