using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagazineAPInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StorefrontOrderShipping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StorefrontOrders_Status",
                table: "StorefrontOrders");

            migrationBuilder.AddColumn<Guid>(
                name: "AcceptedByUserId",
                table: "StorefrontOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CarrierName",
                table: "StorefrontOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CarrierTrackingNumber",
                table: "StorefrontOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PackedByUserId",
                table: "StorefrontOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShippedByUserId",
                table: "StorefrontOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedOnUtc",
                table: "StorefrontOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_StorefrontOrders_Status",
                table: "StorefrontOrders",
                sql: "[Status] IN (1, 2, 3, 4, 5)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StorefrontOrders_Status",
                table: "StorefrontOrders");

            migrationBuilder.DropColumn(
                name: "AcceptedByUserId",
                table: "StorefrontOrders");

            migrationBuilder.DropColumn(
                name: "CarrierName",
                table: "StorefrontOrders");

            migrationBuilder.DropColumn(
                name: "CarrierTrackingNumber",
                table: "StorefrontOrders");

            migrationBuilder.DropColumn(
                name: "PackedByUserId",
                table: "StorefrontOrders");

            migrationBuilder.DropColumn(
                name: "ShippedByUserId",
                table: "StorefrontOrders");

            migrationBuilder.DropColumn(
                name: "ShippedOnUtc",
                table: "StorefrontOrders");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StorefrontOrders_Status",
                table: "StorefrontOrders",
                sql: "[Status] IN (1, 2, 3, 4)");
        }
    }
}
