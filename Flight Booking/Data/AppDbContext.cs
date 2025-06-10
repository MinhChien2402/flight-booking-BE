using Flight_Booking.Model;
using Microsoft.EntityFrameworkCore;

namespace Flight_Booking.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Tickets> Tickets { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Airline> Airlines { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<Plane> Planes { get; set; }
        public DbSet<AirlinePlane> AirlinePlanes { get; set; }
        public DbSet<BookingTicket> BookingTickets { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình bảng Users
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Cấu hình bảng Countries
            modelBuilder.Entity<Country>()
                .Property(c => c.AdditionalCode)
                .HasColumnName("additional_code");

            // Cấu hình bảng Airlines
            modelBuilder.Entity<Airline>()
                .Property(a => a.CountryId)
                .HasColumnName("country_id");

            modelBuilder.Entity<Airline>()
                .HasOne(a => a.Country)
                .WithMany()
                .HasForeignKey(a => a.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình bảng Airports
            modelBuilder.Entity<Airport>()
                .Property(a => a.AdditionalCode)
                .HasColumnName("additional_code");

            // Cấu hình bảng Planes
            modelBuilder.Entity<Plane>()
                .Property(p => p.AdditionalCode)
                .HasColumnName("additional_code");

            // Cấu hình bảng AirlinePlanes
            modelBuilder.Entity<AirlinePlane>()
                .HasKey(ap => new { ap.AirlineId, ap.PlaneId });

            modelBuilder.Entity<AirlinePlane>()
                .HasOne(ap => ap.Airline)
                .WithMany(a => a.AirlinePlanes)
                .HasForeignKey(ap => ap.AirlineId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AirlinePlane>()
                .HasOne(ap => ap.Plane)
                .WithMany(p => p.AirlinePlanes)
                .HasForeignKey(ap => ap.PlaneId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình bảng Tickets
            modelBuilder.Entity<Tickets>()
                .ToTable("tickets")
                .HasKey(t => t.Id);

            modelBuilder.Entity<Tickets>()
                .Property(t => t.Id).HasColumnName("id");
            modelBuilder.Entity<Tickets>()
                .Property(t => t.AirlineId).HasColumnName("airline_id");
            modelBuilder.Entity<Tickets>()
                .Property(t => t.DepartureAirportId).HasColumnName("departure_airport_id");
            modelBuilder.Entity<Tickets>()
                .Property(t => t.ArrivalAirportId).HasColumnName("arrival_airport_id");
            modelBuilder.Entity<Tickets>()
                .Property(t => t.PlaneId).HasColumnName("plane_id");
            modelBuilder.Entity<Tickets>()
                .Property(t => t.DepartureTime).HasColumnName("departure_time");
            modelBuilder.Entity<Tickets>()
                .Property(t => t.ArrivalTime).HasColumnName("arrival_time");
            modelBuilder.Entity<Tickets>()
                .Property(t => t.Stops).HasColumnName("stops");
            modelBuilder.Entity<Tickets>()
                .Property(t => t.Price).HasColumnName("price");
            modelBuilder.Entity<Tickets>()
                .Property(t => t.FlightClass).HasColumnName("flight_class");
            modelBuilder.Entity<Tickets>()
                .Property(t => t.AvailableSeats).HasColumnName("available_seats");

            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.Airline)
                .WithMany()
                .HasForeignKey(t => t.AirlineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.DepartureAirport)
                .WithMany()
                .HasForeignKey(t => t.DepartureAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.ArrivalAirport)
                .WithMany()
                .HasForeignKey(t => t.ArrivalAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.Plane)
                .WithMany()
                .HasForeignKey(t => t.PlaneId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình bảng Bookings
            modelBuilder.Entity<Booking>()
                .ToTable("Bookings")
                .HasKey(b => b.BookingId);

            modelBuilder.Entity<Booking>()
                .Property(b => b.BookingId).HasColumnName("BookingId");
            modelBuilder.Entity<Booking>()
                .Property(b => b.UserId).HasColumnName("UserId");
            modelBuilder.Entity<Booking>()
                .Property(b => b.BookingDate).HasColumnName("BookingDate");
            modelBuilder.Entity<Booking>()
                .Property(b => b.Status).HasColumnName("Status");
            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice).HasColumnName("TotalPrice");

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình bảng BookingTickets
            modelBuilder.Entity<BookingTicket>()
                .ToTable("BookingTickets")
                .HasKey(bt => bt.BookingTicketId);

            modelBuilder.Entity<BookingTicket>()
                .Property(bt => bt.BookingTicketId).HasColumnName("BookingTicketId");
            modelBuilder.Entity<BookingTicket>()
                .Property(bt => bt.BookingId).HasColumnName("BookingId");
            modelBuilder.Entity<BookingTicket>()
                .Property(bt => bt.TicketId).HasColumnName("TicketId");

            modelBuilder.Entity<BookingTicket>()
                .HasOne(bt => bt.Booking)
                .WithMany(b => b.BookingTickets)
                .HasForeignKey(bt => bt.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookingTicket>()
                .HasOne(bt => bt.Ticket)
                .WithMany()
                .HasForeignKey(bt => bt.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình bảng Passengers
            modelBuilder.Entity<Passenger>()
                .ToTable("Passengers")
                .HasKey(p => p.PassengerId);

            modelBuilder.Entity<Passenger>()
                .Property(p => p.PassengerId).HasColumnName("PassengerId");
            modelBuilder.Entity<Passenger>()
                .Property(p => p.BookingId).HasColumnName("BookingId");
            modelBuilder.Entity<Passenger>()
                .Property(p => p.Title).HasColumnName("title");
            modelBuilder.Entity<Passenger>()
                .Property(p => p.FirstName).HasColumnName("first_name");
            modelBuilder.Entity<Passenger>()
                .Property(p => p.LastName).HasColumnName("last_name");
            modelBuilder.Entity<Passenger>()
                .Property(p => p.DateOfBirth).HasColumnName("DateOfBirth");
            modelBuilder.Entity<Passenger>()
                .Property(p => p.PassportNumber).HasColumnName("passport_number");
            modelBuilder.Entity<Passenger>()
                .Property(p => p.PassportExpiry).HasColumnName("passport_expiry");

            modelBuilder.Entity<Passenger>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Passengers)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seeding dữ liệu cho tài khoản admin
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    FullName = "Admin",
                    Email = "admin@example.com",
                    Password = HashPasswordMD5("admin123"), // Mật khẩu admin123 được mã hóa bằng MD5
                    Role = "admin"
                }
            );
        }

        private string HashPasswordMD5(string password)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hashBytes = md5.ComputeHash(inputBytes);
                var sb = new System.Text.StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}