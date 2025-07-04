using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Flight_Booking.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aircraft",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    additional_code = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aircraft", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Airport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    additional_code = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    additional_code = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    PreferredCreditCard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SkyMiles = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RouteSuggestions",
                columns: table => new
                {
                    RouteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    departure_airport_id = table.Column<int>(type: "int", nullable: false),
                    arrival_airport_id = table.Column<int>(type: "int", nullable: false),
                    transfer_points = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteSuggestions", x => x.RouteId);
                    table.ForeignKey(
                        name: "FK_RouteSuggestions_Airport_arrival_airport_id",
                        column: x => x.arrival_airport_id,
                        principalTable: "Airport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteSuggestions_Airport_departure_airport_id",
                        column: x => x.departure_airport_id,
                        principalTable: "Airport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Airline",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    country_id = table.Column<int>(type: "int", nullable: true),
                    Callsign = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airline", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Airline_Country_country_id",
                        column: x => x.country_id,
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AirlineAircraft",
                columns: table => new
                {
                    airline_id = table.Column<int>(type: "int", nullable: false),
                    aircraft_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirlineAircraft", x => new { x.airline_id, x.aircraft_id });
                    table.ForeignKey(
                        name: "FK_AirlineAircraft_Aircraft_aircraft_id",
                        column: x => x.aircraft_id,
                        principalTable: "Aircraft",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AirlineAircraft_Airline_airline_id",
                        column: x => x.airline_id,
                        principalTable: "Airline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlightSchedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    airline_id = table.Column<int>(type: "int", nullable: false),
                    departure_airport_id = table.Column<int>(type: "int", nullable: false),
                    arrival_airport_id = table.Column<int>(type: "int", nullable: false),
                    plane_id = table.Column<int>(type: "int", nullable: false),
                    departure_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    arrival_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    stops = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    flight_class = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    available_seats = table.Column<int>(type: "int", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DynamicPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Distance = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightSchedule_Aircraft_plane_id",
                        column: x => x.plane_id,
                        principalTable: "Aircraft",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlightSchedule_Airline_airline_id",
                        column: x => x.airline_id,
                        principalTable: "Airline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlightSchedule_Airport_arrival_airport_id",
                        column: x => x.arrival_airport_id,
                        principalTable: "Airport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlightSchedule_Airport_departure_airport_id",
                        column: x => x.departure_airport_id,
                        principalTable: "Airport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReservationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalFare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BlockExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CancellationRules = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FlightScheduleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservation_FlightSchedule_FlightScheduleId",
                        column: x => x.FlightScheduleId,
                        principalTable: "FlightSchedule",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reservation_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Passenger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    passport_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    passport_expiry = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passenger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Passenger_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReservationHistory",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationHistory", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_ReservationHistory_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReservationTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    FlightScheduleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationTickets_FlightSchedule_FlightScheduleId",
                        column: x => x.FlightScheduleId,
                        principalTable: "FlightSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReservationTickets_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Aircraft",
                columns: new[] { "Id", "additional_code", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "B78", "B787", "Boeing 787" },
                    { 2, "A32", "A320", "Airbus A320" }
                });

            migrationBuilder.InsertData(
                table: "Airport",
                columns: new[] { "Id", "additional_code", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "TSN", "SGN", "Tan Son Nhat" },
                    { 2, "NBI", "HAN", "Noi Bai" }
                });

            migrationBuilder.InsertData(
                table: "Country",
                columns: new[] { "Id", "additional_code", "code", "Name" },
                values: new object[,]
                {
                    { 1, "VIE", "VNM", "Vietnam" },
                    { 2, "THL", "THA", "Thailand" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Address", "Age", "DateOfBirth", "Email", "FullName", "Password", "PhoneNumber", "PreferredCreditCard", "Role", "Sex", "SkyMiles" },
                values: new object[] { 1, null, null, null, "admin@example.com", "Admin", "0192023a7bbd73250516f069df18b500", null, null, "admin", null, 0m });

            migrationBuilder.InsertData(
                table: "RouteSuggestions",
                columns: new[] { "RouteId", "arrival_airport_id", "departure_airport_id", "transfer_points" },
                values: new object[] { 1, 2, 1, "Hanoi" });

            migrationBuilder.CreateIndex(
                name: "IX_Airline_country_id",
                table: "Airline",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_AirlineAircraft_aircraft_id",
                table: "AirlineAircraft",
                column: "aircraft_id");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSchedule_airline_id",
                table: "FlightSchedule",
                column: "airline_id");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSchedule_arrival_airport_id",
                table: "FlightSchedule",
                column: "arrival_airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSchedule_departure_airport_id",
                table: "FlightSchedule",
                column: "departure_airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSchedule_plane_id",
                table: "FlightSchedule",
                column: "plane_id");

            migrationBuilder.CreateIndex(
                name: "IX_Passenger_ReservationId",
                table: "Passenger",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_FlightScheduleId",
                table: "Reservation",
                column: "FlightScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_UserId",
                table: "Reservation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationHistory_ReservationId",
                table: "ReservationHistory",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationTickets_FlightScheduleId",
                table: "ReservationTickets",
                column: "FlightScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationTickets_ReservationId",
                table: "ReservationTickets",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteSuggestions_arrival_airport_id",
                table: "RouteSuggestions",
                column: "arrival_airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_RouteSuggestions_departure_airport_id",
                table: "RouteSuggestions",
                column: "departure_airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AirlineAircraft");

            migrationBuilder.DropTable(
                name: "Passenger");

            migrationBuilder.DropTable(
                name: "ReservationHistory");

            migrationBuilder.DropTable(
                name: "ReservationTickets");

            migrationBuilder.DropTable(
                name: "RouteSuggestions");

            migrationBuilder.DropTable(
                name: "Reservation");

            migrationBuilder.DropTable(
                name: "FlightSchedule");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Aircraft");

            migrationBuilder.DropTable(
                name: "Airline");

            migrationBuilder.DropTable(
                name: "Airport");

            migrationBuilder.DropTable(
                name: "Country");
        }
    }
}
