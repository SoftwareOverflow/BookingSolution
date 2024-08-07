using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_BusinessEntity_BusinessId",
                table: "Services");

            migrationBuilder.DropTable(
                name: "BusinessEntity");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Services",
                type: "decimal(19,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Business",
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
                    table.PrimaryKey("PK_Business", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Business_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BusinessId",
                table: "AspNetUsers",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Business_AddressId",
                table: "Business",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Business_BusinessId",
                table: "AspNetUsers",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Business_BusinessId",
                table: "Services",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Business_BusinessId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Business_BusinessId",
                table: "Services");

            migrationBuilder.DropTable(
                name: "Business");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BusinessId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Services",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,4)");

            migrationBuilder.CreateTable(
                name: "BusinessEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEntity_AddressId",
                table: "BusinessEntity",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_BusinessEntity_BusinessId",
                table: "Services",
                column: "BusinessId",
                principalTable: "BusinessEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
