using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class ServiceRepeats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableFriday",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "AvailableMonday",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "AvailableSaturday",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "AvailableSunday",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "AvailableThursday",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "AvailableTuesday",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "AvailableWednesday",
                table: "Services");

            migrationBuilder.AddColumn<int>(
                name: "RepeatType",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "Services",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateTable(
                name: "ServiceRepeater",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    DayIdentifier = table.Column<int>(type: "int", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRepeater", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRepeater_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRepeater_ServiceId",
                table: "ServiceRepeater",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceRepeater");

            migrationBuilder.DropColumn(
                name: "RepeatType",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Services");

            migrationBuilder.AddColumn<bool>(
                name: "AvailableFriday",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AvailableMonday",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AvailableSaturday",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AvailableSunday",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AvailableThursday",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AvailableTuesday",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AvailableWednesday",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
