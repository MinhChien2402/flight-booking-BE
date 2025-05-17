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

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Quan hệ cho Airline và Country
            modelBuilder.Entity<Airline>()
                .Property(a => a.CountryId)
                .HasColumnName("country_id");

            modelBuilder.Entity<Country>()
                .Property(c => c.AdditionalCode)
                .HasColumnName("additional_code");

            // Quan hệ cho AirlinePlane
            modelBuilder.Entity<AirlinePlane>()
                .HasKey(ap => new { ap.AirlineId, ap.PlaneId });

            modelBuilder.Entity<AirlinePlane>()
                .HasOne(ap => ap.Airline)
                .WithMany(a => a.AirlinePlanes)
                .HasForeignKey(ap => ap.AirlineId);

            modelBuilder.Entity<AirlinePlane>()
                .HasOne(ap => ap.Plane)
                .WithMany(p => p.AirlinePlanes)
                .HasForeignKey(ap => ap.PlaneId);

            // Quan hệ cho Tickets
            modelBuilder.Entity<Tickets>(entity =>
            {
                entity.ToTable("Tickets");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.AirlineId).HasColumnName("airline_id");
                entity.Property(e => e.DepartureAirportId).HasColumnName("departure_airport_id");
                entity.Property(e => e.ArrivalAirportId).HasColumnName("arrival_airport_id");
                entity.Property(e => e.PlaneId).HasColumnName("plane_id");
                entity.Property(e => e.DepartureTime).HasColumnName("departure_time");
                entity.Property(e => e.ArrivalTime).HasColumnName("arrival_time");
                entity.Property(e => e.Stops).HasColumnName("stops");
                entity.Property(e => e.Price).HasColumnName("price");
                entity.Property(e => e.FlightClass).HasColumnName("flight_class");
                entity.Property(e => e.AvailableSeats).HasColumnName("available_seats");
            });

            // Quan hệ cho Booking, Passenger, User, Tickets
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Ticket)
                .WithMany()
                .HasForeignKey(b => b.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

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
                    Password = "482c811da5db4bc6d497fa98491e38", // Mật khẩu admin123 được mã hóa bằng MD5
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