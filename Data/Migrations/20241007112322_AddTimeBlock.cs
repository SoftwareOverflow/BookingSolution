using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeBlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "RepeatType",
                table: "Services",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "TimeBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RepeatType = table.Column<byte>(type: "tinyint", nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeBlocks_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeBlockException",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeBlockId = table.Column<int>(type: "int", nullable: true),
                    DateToReplace = table.Column<DateOnly>(type: "date", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeBlockException", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeBlockException_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimeBlockException_TimeBlocks_TimeBlockId",
                        column: x => x.TimeBlockId,
                        principalTable: "TimeBlocks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TimeBlockRepeater",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeBlockId = table.Column<int>(type: "int", nullable: false),
                    DayIdentifier = table.Column<int>(type: "int", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeBlockRepeater", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeBlockRepeater_TimeBlocks_TimeBlockId",
                        column: x => x.TimeBlockId,
                        principalTable: "TimeBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeBlockException_BusinessId",
                table: "TimeBlockException",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeBlockException_TimeBlockId",
                table: "TimeBlockException",
                column: "TimeBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeBlockRepeater_TimeBlockId",
                table: "TimeBlockRepeater",
                column: "TimeBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeBlocks_BusinessId",
                table: "TimeBlocks",
                column: "BusinessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeBlockException");

            migrationBuilder.DropTable(
                name: "TimeBlockRepeater");

            migrationBuilder.DropTable(
                name: "TimeBlocks");

            migrationBuilder.AlterColumn<int>(
                name: "RepeatType",
                table: "Services",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");
        }
    }
}
