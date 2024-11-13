using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class TimeBlockExceptionsRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeBlockException_Businesses_BusinessId",
                table: "TimeBlockException");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeBlockException_TimeBlocks_TimeBlockId",
                table: "TimeBlockException");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeBlockException",
                table: "TimeBlockException");

            migrationBuilder.RenameTable(
                name: "TimeBlockException",
                newName: "TimeBlockExceptions");

            migrationBuilder.RenameIndex(
                name: "IX_TimeBlockException_TimeBlockId",
                table: "TimeBlockExceptions",
                newName: "IX_TimeBlockExceptions_TimeBlockId");

            migrationBuilder.RenameIndex(
                name: "IX_TimeBlockException_BusinessId",
                table: "TimeBlockExceptions",
                newName: "IX_TimeBlockExceptions_BusinessId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeBlockExceptions",
                table: "TimeBlockExceptions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeBlockExceptions_Businesses_BusinessId",
                table: "TimeBlockExceptions",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeBlockExceptions_TimeBlocks_TimeBlockId",
                table: "TimeBlockExceptions",
                column: "TimeBlockId",
                principalTable: "TimeBlocks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeBlockExceptions_Businesses_BusinessId",
                table: "TimeBlockExceptions");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeBlockExceptions_TimeBlocks_TimeBlockId",
                table: "TimeBlockExceptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeBlockExceptions",
                table: "TimeBlockExceptions");

            migrationBuilder.RenameTable(
                name: "TimeBlockExceptions",
                newName: "TimeBlockException");

            migrationBuilder.RenameIndex(
                name: "IX_TimeBlockExceptions_TimeBlockId",
                table: "TimeBlockException",
                newName: "IX_TimeBlockException_TimeBlockId");

            migrationBuilder.RenameIndex(
                name: "IX_TimeBlockExceptions_BusinessId",
                table: "TimeBlockException",
                newName: "IX_TimeBlockException_BusinessId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeBlockException",
                table: "TimeBlockException",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeBlockException_Businesses_BusinessId",
                table: "TimeBlockException",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeBlockException_TimeBlocks_TimeBlockId",
                table: "TimeBlockException",
                column: "TimeBlockId",
                principalTable: "TimeBlocks",
                principalColumn: "Id");
        }
    }
}
