using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameBusinessUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessUser_Businesses_BusinessId",
                table: "BusinessUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BusinessUser",
                table: "BusinessUser");

            migrationBuilder.RenameTable(
                name: "BusinessUser",
                newName: "BusinessUsers");

            migrationBuilder.RenameIndex(
                name: "IX_BusinessUser_BusinessId",
                table: "BusinessUsers",
                newName: "IX_BusinessUsers_BusinessId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BusinessUsers",
                table: "BusinessUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessUsers_Businesses_BusinessId",
                table: "BusinessUsers",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessUsers_Businesses_BusinessId",
                table: "BusinessUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BusinessUsers",
                table: "BusinessUsers");

            migrationBuilder.RenameTable(
                name: "BusinessUsers",
                newName: "BusinessUser");

            migrationBuilder.RenameIndex(
                name: "IX_BusinessUsers_BusinessId",
                table: "BusinessUser",
                newName: "IX_BusinessUser_BusinessId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BusinessUser",
                table: "BusinessUser",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessUser_Businesses_BusinessId",
                table: "BusinessUser",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
