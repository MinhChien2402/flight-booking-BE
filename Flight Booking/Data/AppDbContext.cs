using Flight_Booking.Model;
using Microsoft.EntityFrameworkCore;

namespace Flight_Booking.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<FlightSchedule> FlightSchedules { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Airline> Airline { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<Aircraft> Aircrafts { get; set; }
        public DbSet<AirlineAircraft> AirlinePlanes { get; set; }
        public DbSet<ReservationTicket> ReservationTickets { get; set; }
        public DbSet<ReservationHistory> ReservationHistories { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình bảng Users
            modelBuilder.Entity<User>()
                .ToTable("User")
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .Property(u => u.Address).HasColumnName("Address");
            modelBuilder.Entity<User>()
                .Property(u => u.Sex).HasColumnName("Sex");
            modelBuilder.Entity<User>()
                .Property(u => u.Age).HasColumnName("Age");
            modelBuilder.Entity<User>()
                .Property(u => u.PreferredCreditCard).HasColumnName("PreferredCreditCard");
            modelBuilder.Entity<User>()
                .Property(u => u.SkyMiles).HasColumnName("SkyMiles");

            // Cấu hình bảng Countries
            modelBuilder.Entity<Country>()
                .ToTable("Country")
                .Property(c => c.AdditionalCode)
                .HasColumnName("additional_code");

            // Cấu hình bảng Airlines
            modelBuilder.Entity<Airline>()
                .ToTable("Airline")
                .Property(a => a.CountryId)
                .HasColumnName("country_id")
                .IsRequired(false);
            modelBuilder.Entity<Airline>()
                .HasOne(a => a.Country)
                .WithMany()
                .HasForeignKey(a => a.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình bảng Airports
            modelBuilder.Entity<Airport>()
                .ToTable("Airport")
                .Property(a => a.AdditionalCode)
                .HasColumnName("additional_code");

            // Cấu hình bảng Aircraft
            modelBuilder.Entity<Aircraft>()
                .ToTable("Aircraft")
                .Property(p => p.AdditionalCode)
                .HasColumnName("additional_code");

            // Cấu hình bảng AirlineAircraft
            modelBuilder.Entity<AirlineAircraft>()
                .ToTable("AirlineAircraft")
                .HasKey(ap => new { ap.AirlineId, ap.AircraftId });

            modelBuilder.Entity<AirlineAircraft>()
                .HasOne(ap => ap.Airline)
                .WithMany(a => a.AirlinePlanes)
                .HasForeignKey(ap => ap.AirlineId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AirlineAircraft>()
                .HasOne(ap => ap.Aircraft)
                .WithMany(a => a.AirlineAircrafts)
                .HasForeignKey(ap => ap.AircraftId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình bảng FlightSchedule
            modelBuilder.Entity<FlightSchedule>()
                .ToTable("FlightSchedule")
                .HasKey(t => t.Id);
            modelBuilder.Entity<FlightSchedule>()
                .Property(t => t.Id).HasColumnName("Id");
            modelBuilder.Entity<FlightSchedule>()
                .Property(t => t.AirlineId).HasColumnName("airline_id");
            modelBuilder.Entity<FlightSchedule>()
                .Property(t => t.DepartureAirportId).HasColumnName("departure_airport_id");
            modelBuilder.Entity<FlightSchedule>()
                .Property(t => t.ArrivalAirportId).HasColumnName("arrival_airport_id");
            modelBuilder.Entity<FlightSchedule>()
                .Property(t => t.AircraftId).HasColumnName("plane_id");
            modelBuilder.Entity<FlightSchedule>()
                .Property(t => t.DepartureTime).HasColumnName("departure_time").IsRequired(false);
            modelBuilder.Entity<FlightSchedule>()
                .Property(t => t.ArrivalTime).HasColumnName("arrival_time").IsRequired(false);
            modelBuilder.Entity<FlightSchedule>()
                .Property(t => t.Stops).HasColumnName("stops");
            modelBuilder.Entity<FlightSchedule>()
                .Property(t => t.Price).HasColumnName("price");
            modelBuilder.Entity<FlightSchedule>()
                .Property(t => t.FlightClass).HasColumnName("flight_class");
            modelBuilder.Entity<FlightSchedule>()
                .Property(t => t.AvailableSeats).HasColumnName("available_seats");
            modelBuilder.Entity<FlightSchedule>()
                .Property(t => t.LastUpdate).HasColumnName("LastUpdate");
            modelBuilder.Entity<FlightSchedule>()
                .Property(t => t.DynamicPrice).HasColumnName("DynamicPrice");
            modelBuilder.Entity<FlightSchedule>()
                .HasOne(t => t.Airline)
                .WithMany()
                .HasForeignKey(t => t.AirlineId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FlightSchedule>()
                .HasOne(t => t.DepartureAirport)
                .WithMany()
                .HasForeignKey(t => t.DepartureAirportId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FlightSchedule>()
                .HasOne(t => t.ArrivalAirport)
                .WithMany()
                .HasForeignKey(t => t.ArrivalAirportId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FlightSchedule>()
                .HasOne(t => t.Aircraft)
                .WithMany()
                .HasForeignKey(t => t.AircraftId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình bảng ReservationHistory
            modelBuilder.Entity<ReservationHistory>()
                .ToTable("ReservationHistory")
                .HasKey(rh => rh.HistoryId);
            modelBuilder.Entity<ReservationHistory>()
                .Property(rh => rh.HistoryId).HasColumnName("HistoryId");
            modelBuilder.Entity<ReservationHistory>()
                .Property(rh => rh.ReservationId).HasColumnName("ReservationId");
            modelBuilder.Entity<ReservationHistory>()
                .Property(rh => rh.ActionType).HasColumnName("ActionType");
            modelBuilder.Entity<ReservationHistory>()
                .Property(rh => rh.OldDate).HasColumnName("OldDate");
            modelBuilder.Entity<ReservationHistory>()
                .Property(rh => rh.NewDate).HasColumnName("NewDate");
            modelBuilder.Entity<ReservationHistory>()
                .Property(rh => rh.RefundAmount).HasColumnName("RefundAmount").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ReservationHistory>()
                .Property(rh => rh.ActionDate).HasColumnName("ActionDate");
            modelBuilder.Entity<ReservationHistory>()
                .HasOne(rh => rh.Reservation)
                .WithMany()
                .HasForeignKey(rh => rh.ReservationId)
                .OnDelete(DeleteBehavior.Restrict);

           

            // Seeding dữ liệu
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "Admin",
                    Email = "admin@example.com",
                    Password = HashPasswordMD5("admin123"),
                    Role = "admin"
                }
            );

            modelBuilder.Entity<Aircraft>().HasData(
                new Aircraft { Id = 1, Name = "Boeing 787", Code = "B787", AdditionalCode = "B78" },
                new Aircraft { Id = 2, Name = "Airbus A320", Code = "A320", AdditionalCode = "A32" }
            );

            modelBuilder.Entity<Airport>().HasData(
                new Airport { Id = 1, Name = "Tan Son Nhat", Code = "SGN", AdditionalCode = "TSN" },
                new Airport { Id = 2, Name = "Noi Bai", Code = "HAN", AdditionalCode = "NBI" }
            );

            modelBuilder.Entity<Country>().HasData(
                new Country { Id = 1, Name = "Vietnam", Code = "VNM", AdditionalCode = "VIE" },
                new Country { Id = 2, Name = "Thailand", Code = "THA", AdditionalCode = "THL" }
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