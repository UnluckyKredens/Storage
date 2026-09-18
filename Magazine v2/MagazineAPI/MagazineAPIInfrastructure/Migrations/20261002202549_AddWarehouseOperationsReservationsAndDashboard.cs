using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagazineAPInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseOperationsReservationsAndDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StockMovements_Type",
                table: "StockMovements");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Shipments_Status",
                table: "Shipments");

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumQuantity",
                table: "Products",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OptimumQuantity",
                table: "Products",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StockReservations",
                columns: table => new
                {
                    StockReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReleasedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReservations", x => x.StockReservationId);
                    table.CheckConstraint("CK_StockReservations_QuantityValues", "[Quantity] > 0 AND [ReleasedQuantity] >= 0 AND [ReleasedQuantity] <= [Quantity]");
                    table.CheckConstraint("CK_StockReservations_Status", "[Status] IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_StockReservations_Inventory_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventory",
                        principalColumn: "InventoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockReservations_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockReservations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockReservations_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockReservations_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseOperations",
                columns: table => new
                {
                    WarehouseOperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseOperations", x => x.WarehouseOperationId);
                    table.CheckConstraint("CK_WarehouseOperations_Status", "[Status] IN (1, 2)");
                    table.CheckConstraint("CK_WarehouseOperations_Type", "[Type] IN (1, 2, 3, 4, 5)");
                    table.ForeignKey(
                        name: "FK_WarehouseOperations_Users_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseOperations_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseOperations_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseOperationItems",
                columns: table => new
                {
                    WarehouseOperationItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseOperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DestinationLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TargetQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseOperationItems", x => x.WarehouseOperationItemId);
                    table.CheckConstraint("CK_WarehouseOperationItems_Quantity", "[Quantity] >= 0 AND ([TargetQuantity] IS NULL OR [TargetQuantity] >= 0)");
                    table.ForeignKey(
                        name: "FK_WarehouseOperationItems_Locations_DestinationLocationId",
                        column: x => x.DestinationLocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseOperationItems_Locations_SourceLocationId",
                        column: x => x.SourceLocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseOperationItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseOperationItems_WarehouseOperations_WarehouseOperationId",
                        column: x => x.WarehouseOperationId,
                        principalTable: "WarehouseOperations",
                        principalColumn: "WarehouseOperationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockMovements_Type",
                table: "StockMovements",
                sql: "[Type] IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Shipments_Status",
                table: "Shipments",
                sql: "[Status] IN (1, 2, 3, 4)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_QuantityThresholds",
                table: "Products",
                sql: "[MinimumQuantity] >= 0 AND ([OptimumQuantity] IS NULL OR [OptimumQuantity] >= [MinimumQuantity])");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_CreatedByUserId",
                table: "StockReservations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_InventoryId",
                table: "StockReservations",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_LocationId",
                table: "StockReservations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_ProductId",
                table: "StockReservations",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_SourceType_SourceId",
                table: "StockReservations",
                columns: new[] { "SourceType", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_WarehouseId_ProductId_Status",
                table: "StockReservations",
                columns: new[] { "WarehouseId", "ProductId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseOperationItems_DestinationLocationId",
                table: "WarehouseOperationItems",
                column: "DestinationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseOperationItems_ProductId",
                table: "WarehouseOperationItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseOperationItems_SourceLocationId",
                table: "WarehouseOperationItems",
                column: "SourceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseOperationItems_WarehouseOperationId",
                table: "WarehouseOperationItems",
                column: "WarehouseOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseOperations_CompletedByUserId",
                table: "WarehouseOperations",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseOperations_CreatedByUserId",
                table: "WarehouseOperations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseOperations_Number",
                table: "WarehouseOperations",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseOperations_WarehouseId_CompletedOnUtc",
                table: "WarehouseOperations",
                columns: new[] { "WarehouseId", "CompletedOnUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockReservations");

            migrationBuilder.DropTable(
                name: "WarehouseOperationItems");

            migrationBuilder.DropTable(
                name: "WarehouseOperations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockMovements_Type",
                table: "StockMovements");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Shipments_Status",
                table: "Shipments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_QuantityThresholds",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MinimumQuantity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OptimumQuantity",
                table: "Products");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockMovements_Type",
                table: "StockMovements",
                sql: "[Type] IN (1, 2, 3, 4, 5, 6, 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Shipments_Status",
                table: "Shipments",
                sql: "[Status] IN (1, 2, 3)");
        }
    }
}
