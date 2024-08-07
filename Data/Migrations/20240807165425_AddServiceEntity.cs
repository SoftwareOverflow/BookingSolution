using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessEntity_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EarliestTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    LatestTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    BookingFrequencyMins = table.Column<int>(type: "int", nullable: false),
                    DurationMins = table.Column<int>(type: "int", nullable: false),
                    PaddingStartMins = table.Column<int>(type: "int", nullable: false),
                    PaddingEndMins = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableMonday = table.Column<bool>(type: "bit", nullable: false),
                    AvailableTuesday = table.Column<bool>(type: "bit", nullable: false),
                    AvailableWednesday = table.Column<bool>(type: "bit", nullable: false),
                    AvailableThursday = table.Column<bool>(type: "bit", nullable: false),
                    AvailableFriday = table.Column<bool>(type: "bit", nullable: false),
                    AvailableSaturday = table.Column<bool>(type: "bit", nullable: false),
                    AvailableSunday = table.Column<bool>(type: "bit", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_BusinessEntity_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "BusinessEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEntity_AddressId",
                table: "BusinessEntity",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_BusinessId",
                table: "Services",
                column: "BusinessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "BusinessEntity");

            migrationBuilder.DropTable(
                name: "Address");
        }
    }
}
