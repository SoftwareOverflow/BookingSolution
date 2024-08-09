using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameAddressLine1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Business_BusinessId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Business_Address_AddressId",
                table: "Business");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Business_BusinessId",
                table: "Services");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Business",
                table: "Business");

            migrationBuilder.RenameTable(
                name: "Business",
                newName: "Businesses");

            migrationBuilder.RenameColumn(
                name: "Address2",
                table: "Address",
                newName: "Address3");

            migrationBuilder.RenameColumn(
                name: "Address1",
                table: "Address",
                newName: "Address2");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Address",
                newName: "Address1");

            migrationBuilder.RenameIndex(
                name: "IX_Business_AddressId",
                table: "Businesses",
                newName: "IX_Businesses_AddressId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Businesses",
                table: "Businesses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Businesses_BusinessId",
                table: "AspNetUsers",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_Address_AddressId",
                table: "Businesses",
                column: "AddressId",
                principalTable: "Address",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Businesses_BusinessId",
                table: "Services",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Businesses_BusinessId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_Address_AddressId",
                table: "Businesses");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Businesses_BusinessId",
                table: "Services");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Businesses",
                table: "Businesses");

            migrationBuilder.RenameTable(
                name: "Businesses",
                newName: "Business");

            migrationBuilder.RenameColumn(
                name: "Address1",
                table: "Address",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Address2",
                table: "Address",
                newName: "Address1");

            migrationBuilder.RenameColumn(
                name: "Address3",
                table: "Address",
                newName: "Address2");

            migrationBuilder.RenameIndex(
                name: "IX_Businesses_AddressId",
                table: "Business",
                newName: "IX_Business_AddressId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Business",
                table: "Business",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Business_BusinessId",
                table: "AspNetUsers",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Business_Address_AddressId",
                table: "Business",
                column: "AddressId",
                principalTable: "Address",
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
    }
}
