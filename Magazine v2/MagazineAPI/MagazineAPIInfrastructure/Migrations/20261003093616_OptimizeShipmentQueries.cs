using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagazineAPInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeShipmentQueries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_DestinationWarehouseId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_SourceWarehouseId",
                table: "Shipments");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_DestinationWarehouseId_CreatedOnUtc",
                table: "Shipments",
                columns: new[] { "DestinationWarehouseId", "CreatedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_SourceWarehouseId_CreatedOnUtc",
                table: "Shipments",
                columns: new[] { "SourceWarehouseId", "CreatedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Status_CreatedOnUtc",
                table: "Shipments",
                columns: new[] { "Status", "CreatedOnUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_DestinationWarehouseId_CreatedOnUtc",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_SourceWarehouseId_CreatedOnUtc",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_Status_CreatedOnUtc",
                table: "Shipments");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_DestinationWarehouseId",
                table: "Shipments",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_SourceWarehouseId",
                table: "Shipments",
                column: "SourceWarehouseId");
        }
    }
}
