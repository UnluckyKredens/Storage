using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagazineAPInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockDocumentConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockDocumentItems_StockDocumentId",
                table: "StockDocumentItems");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockDocuments_Status",
                table: "StockDocuments",
                sql: "[Status] IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockDocuments_Type",
                table: "StockDocuments",
                sql: "[Type] IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_StockDocumentItems_StockDocumentId_ProductId_LocationId",
                table: "StockDocumentItems",
                columns: new[] { "StockDocumentId", "ProductId", "LocationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StockDocuments_Status",
                table: "StockDocuments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockDocuments_Type",
                table: "StockDocuments");

            migrationBuilder.DropIndex(
                name: "IX_StockDocumentItems_StockDocumentId_ProductId_LocationId",
                table: "StockDocumentItems");

            migrationBuilder.CreateIndex(
                name: "IX_StockDocumentItems_StockDocumentId",
                table: "StockDocumentItems",
                column: "StockDocumentId");
        }
    }
}
