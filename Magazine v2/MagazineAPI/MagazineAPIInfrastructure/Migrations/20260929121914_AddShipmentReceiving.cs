using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagazineAPInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShipmentReceiving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Shipments_Status",
                table: "Shipments");

            migrationBuilder.AddColumn<Guid>(
                name: "ReceivedByUserId",
                table: "Shipments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedOnUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ReceivedByUserId",
                table: "Shipments",
                column: "ReceivedByUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Shipments_Status",
                table: "Shipments",
                sql: "[Status] IN (1, 2, 3)");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Users_ReceivedByUserId",
                table: "Shipments",
                column: "ReceivedByUserId",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Users_ReceivedByUserId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ReceivedByUserId",
                table: "Shipments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Shipments_Status",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReceivedByUserId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReceivedOnUtc",
                table: "Shipments");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Shipments_Status",
                table: "Shipments",
                sql: "[Status] IN (1, 2)");
        }
    }
}
