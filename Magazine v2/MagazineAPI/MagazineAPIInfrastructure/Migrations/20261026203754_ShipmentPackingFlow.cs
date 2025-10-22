using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagazineAPInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ShipmentPackingFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Shipments_Status",
                table: "Shipments");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Shipments_Status",
                table: "Shipments",
                sql: "[Status] IN (1, 2, 3, 4, 5, 6, 7)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Shipments_Status",
                table: "Shipments");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Shipments_Status",
                table: "Shipments",
                sql: "[Status] IN (1, 2, 3, 4, 5)");
        }
    }
}
