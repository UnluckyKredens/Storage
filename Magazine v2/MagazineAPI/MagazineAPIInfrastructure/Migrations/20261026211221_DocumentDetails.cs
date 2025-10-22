using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagazineAPInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DocumentDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "Warehouses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PackedByUserId",
                table: "Shipments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PackedOnUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SentByUserId",
                table: "Shipments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentOnUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_PackedByUserId",
                table: "Shipments",
                column: "PackedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_SentByUserId",
                table: "Shipments",
                column: "SentByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Users_PackedByUserId",
                table: "Shipments",
                column: "PackedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Users_SentByUserId",
                table: "Shipments",
                column: "SentByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Users_PackedByUserId",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Users_SentByUserId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_PackedByUserId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_SentByUserId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "PackedByUserId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PackedOnUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "SentByUserId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "SentOnUtc",
                table: "Shipments");
        }
    }
}
