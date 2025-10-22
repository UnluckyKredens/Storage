using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagazineAPInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizePrimaryKeysToId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_Locations_LocationID",
                table: "Inventory");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_Products_ProductID",
                table: "Inventory");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Warehouses_WarehouseID",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryID",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_UnitsOfMeasure_UnitOfMeasureID",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_StorefrontOrderAllocations_Inventory_InventoryID",
                table: "StorefrontOrderAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_StorefrontOrderAllocations_Locations_LocationID",
                table: "StorefrontOrderAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_StorefrontOrderAllocations_Products_ProductID",
                table: "StorefrontOrderAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_StorefrontOrderAllocations_Warehouses_WarehouseID",
                table: "StorefrontOrderAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_StorefrontOrders_Warehouses_AssignedWarehouseID",
                table: "StorefrontOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleID",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Warehouses_WarehouseID",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_WarehouseAssignment",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "WarehouseID",
                table: "Warehouses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "WarehouseOperationId",
                table: "WarehouseOperations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "WarehouseOperationItemId",
                table: "WarehouseOperationItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "WarehouseID",
                table: "Users",
                newName: "WarehouseId");

            migrationBuilder.RenameColumn(
                name: "RoleID",
                table: "Users",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Users_WarehouseID",
                table: "Users",
                newName: "IX_Users_WarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_RoleID",
                table: "Users",
                newName: "IX_Users_RoleId");

            migrationBuilder.RenameColumn(
                name: "UnitOfMeasureID",
                table: "UnitsOfMeasure",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "AssignedWarehouseID",
                table: "StorefrontOrders",
                newName: "AssignedWarehouseId");

            migrationBuilder.RenameColumn(
                name: "StorefrontOrderId",
                table: "StorefrontOrders",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_StorefrontOrders_AssignedWarehouseID_Status",
                table: "StorefrontOrders",
                newName: "IX_StorefrontOrders_AssignedWarehouseId_Status");

            migrationBuilder.RenameColumn(
                name: "StorefrontOrderItemId",
                table: "StorefrontOrderItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "WarehouseID",
                table: "StorefrontOrderAllocations",
                newName: "WarehouseId");

            migrationBuilder.RenameColumn(
                name: "ProductID",
                table: "StorefrontOrderAllocations",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "LocationID",
                table: "StorefrontOrderAllocations",
                newName: "LocationId");

            migrationBuilder.RenameColumn(
                name: "InventoryID",
                table: "StorefrontOrderAllocations",
                newName: "InventoryId");

            migrationBuilder.RenameColumn(
                name: "StorefrontOrderAllocationId",
                table: "StorefrontOrderAllocations",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_StorefrontOrderAllocations_WarehouseID_ProductID",
                table: "StorefrontOrderAllocations",
                newName: "IX_StorefrontOrderAllocations_WarehouseId_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_StorefrontOrderAllocations_ProductID",
                table: "StorefrontOrderAllocations",
                newName: "IX_StorefrontOrderAllocations_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_StorefrontOrderAllocations_LocationID",
                table: "StorefrontOrderAllocations",
                newName: "IX_StorefrontOrderAllocations_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_StorefrontOrderAllocations_InventoryID",
                table: "StorefrontOrderAllocations",
                newName: "IX_StorefrontOrderAllocations_InventoryId");

            migrationBuilder.RenameColumn(
                name: "StockReservationId",
                table: "StockReservations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "StockMovementId",
                table: "StockMovements",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ShipmentId",
                table: "Shipments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ShipmentItemId",
                table: "ShipmentItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderId",
                table: "PurchaseOrders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderItemId",
                table: "PurchaseOrderItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "UnitOfMeasureID",
                table: "Products",
                newName: "UnitOfMeasureId");

            migrationBuilder.RenameColumn(
                name: "CategoryID",
                table: "Products",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "ProductID",
                table: "Products",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Products_UnitOfMeasureID",
                table: "Products",
                newName: "IX_Products_UnitOfMeasureId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CategoryID",
                table: "Products",
                newName: "IX_Products_CategoryId");

            migrationBuilder.RenameColumn(
                name: "WarehouseID",
                table: "Locations",
                newName: "WarehouseId");

            migrationBuilder.RenameColumn(
                name: "LocationID",
                table: "Locations",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Locations_WarehouseID_LocationCode",
                table: "Locations",
                newName: "IX_Locations_WarehouseId_LocationCode");

            migrationBuilder.RenameColumn(
                name: "ProductID",
                table: "Inventory",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "LocationID",
                table: "Inventory",
                newName: "LocationId");

            migrationBuilder.RenameColumn(
                name: "InventoryID",
                table: "Inventory",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Inventory_ProductID_LocationID",
                table: "Inventory",
                newName: "IX_Inventory_ProductId_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_Inventory_LocationID",
                table: "Inventory",
                newName: "IX_Inventory_LocationId");

            migrationBuilder.RenameColumn(
                name: "DocumentNumberSequenceId",
                table: "DocumentNumberSequences",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ContractorID",
                table: "Contractors",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "CategoryID",
                table: "Categories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "AuditLogId",
                table: "AuditLogs",
                newName: "Id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_WarehouseAssignment",
                table: "Users",
                sql: "[RoleId] = '10000000-0000-0000-0000-000000000001' OR [WarehouseId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_Locations_LocationId",
                table: "Inventory",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_Products_ProductId",
                table: "Inventory",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Warehouses_WarehouseId",
                table: "Locations",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_UnitsOfMeasure_UnitOfMeasureId",
                table: "Products",
                column: "UnitOfMeasureId",
                principalTable: "UnitsOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorefrontOrderAllocations_Inventory_InventoryId",
                table: "StorefrontOrderAllocations",
                column: "InventoryId",
                principalTable: "Inventory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorefrontOrderAllocations_Locations_LocationId",
                table: "StorefrontOrderAllocations",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorefrontOrderAllocations_Products_ProductId",
                table: "StorefrontOrderAllocations",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorefrontOrderAllocations_Warehouses_WarehouseId",
                table: "StorefrontOrderAllocations",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorefrontOrders_Warehouses_AssignedWarehouseId",
                table: "StorefrontOrders",
                column: "AssignedWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Warehouses_WarehouseId",
                table: "Users",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_Locations_LocationId",
                table: "Inventory");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_Products_ProductId",
                table: "Inventory");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Warehouses_WarehouseId",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_UnitsOfMeasure_UnitOfMeasureId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_StorefrontOrderAllocations_Inventory_InventoryId",
                table: "StorefrontOrderAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_StorefrontOrderAllocations_Locations_LocationId",
                table: "StorefrontOrderAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_StorefrontOrderAllocations_Products_ProductId",
                table: "StorefrontOrderAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_StorefrontOrderAllocations_Warehouses_WarehouseId",
                table: "StorefrontOrderAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_StorefrontOrders_Warehouses_AssignedWarehouseId",
                table: "StorefrontOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Warehouses_WarehouseId",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_WarehouseAssignment",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Warehouses",
                newName: "WarehouseID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "WarehouseOperations",
                newName: "WarehouseOperationId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "WarehouseOperationItems",
                newName: "WarehouseOperationItemId");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "Users",
                newName: "WarehouseID");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "Users",
                newName: "RoleID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Users_WarehouseId",
                table: "Users",
                newName: "IX_Users_WarehouseID");

            migrationBuilder.RenameIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                newName: "IX_Users_RoleID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UnitsOfMeasure",
                newName: "UnitOfMeasureID");

            migrationBuilder.RenameColumn(
                name: "AssignedWarehouseId",
                table: "StorefrontOrders",
                newName: "AssignedWarehouseID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "StorefrontOrders",
                newName: "StorefrontOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_StorefrontOrders_AssignedWarehouseId_Status",
                table: "StorefrontOrders",
                newName: "IX_StorefrontOrders_AssignedWarehouseID_Status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "StorefrontOrderItems",
                newName: "StorefrontOrderItemId");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "StorefrontOrderAllocations",
                newName: "WarehouseID");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "StorefrontOrderAllocations",
                newName: "ProductID");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "StorefrontOrderAllocations",
                newName: "LocationID");

            migrationBuilder.RenameColumn(
                name: "InventoryId",
                table: "StorefrontOrderAllocations",
                newName: "InventoryID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "StorefrontOrderAllocations",
                newName: "StorefrontOrderAllocationId");

            migrationBuilder.RenameIndex(
                name: "IX_StorefrontOrderAllocations_WarehouseId_ProductId",
                table: "StorefrontOrderAllocations",
                newName: "IX_StorefrontOrderAllocations_WarehouseID_ProductID");

            migrationBuilder.RenameIndex(
                name: "IX_StorefrontOrderAllocations_ProductId",
                table: "StorefrontOrderAllocations",
                newName: "IX_StorefrontOrderAllocations_ProductID");

            migrationBuilder.RenameIndex(
                name: "IX_StorefrontOrderAllocations_LocationId",
                table: "StorefrontOrderAllocations",
                newName: "IX_StorefrontOrderAllocations_LocationID");

            migrationBuilder.RenameIndex(
                name: "IX_StorefrontOrderAllocations_InventoryId",
                table: "StorefrontOrderAllocations",
                newName: "IX_StorefrontOrderAllocations_InventoryID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "StockReservations",
                newName: "StockReservationId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "StockMovements",
                newName: "StockMovementId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Shipments",
                newName: "ShipmentId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ShipmentItems",
                newName: "ShipmentItemId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PurchaseOrders",
                newName: "PurchaseOrderId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PurchaseOrderItems",
                newName: "PurchaseOrderItemId");

            migrationBuilder.RenameColumn(
                name: "UnitOfMeasureId",
                table: "Products",
                newName: "UnitOfMeasureID");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Products",
                newName: "CategoryID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Products",
                newName: "ProductID");

            migrationBuilder.RenameIndex(
                name: "IX_Products_UnitOfMeasureId",
                table: "Products",
                newName: "IX_Products_UnitOfMeasureID");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                newName: "IX_Products_CategoryID");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "Locations",
                newName: "WarehouseID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Locations",
                newName: "LocationID");

            migrationBuilder.RenameIndex(
                name: "IX_Locations_WarehouseId_LocationCode",
                table: "Locations",
                newName: "IX_Locations_WarehouseID_LocationCode");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Inventory",
                newName: "ProductID");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "Inventory",
                newName: "LocationID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Inventory",
                newName: "InventoryID");

            migrationBuilder.RenameIndex(
                name: "IX_Inventory_ProductId_LocationId",
                table: "Inventory",
                newName: "IX_Inventory_ProductID_LocationID");

            migrationBuilder.RenameIndex(
                name: "IX_Inventory_LocationId",
                table: "Inventory",
                newName: "IX_Inventory_LocationID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "DocumentNumberSequences",
                newName: "DocumentNumberSequenceId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Contractors",
                newName: "ContractorID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Categories",
                newName: "CategoryID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AuditLogs",
                newName: "AuditLogId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_WarehouseAssignment",
                table: "Users",
                sql: "[RoleID] = '10000000-0000-0000-0000-000000000001' OR [WarehouseID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_Locations_LocationID",
                table: "Inventory",
                column: "LocationID",
                principalTable: "Locations",
                principalColumn: "LocationID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_Products_ProductID",
                table: "Inventory",
                column: "ProductID",
                principalTable: "Products",
                principalColumn: "ProductID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Warehouses_WarehouseID",
                table: "Locations",
                column: "WarehouseID",
                principalTable: "Warehouses",
                principalColumn: "WarehouseID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryID",
                table: "Products",
                column: "CategoryID",
                principalTable: "Categories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_UnitsOfMeasure_UnitOfMeasureID",
                table: "Products",
                column: "UnitOfMeasureID",
                principalTable: "UnitsOfMeasure",
                principalColumn: "UnitOfMeasureID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorefrontOrderAllocations_Inventory_InventoryID",
                table: "StorefrontOrderAllocations",
                column: "InventoryID",
                principalTable: "Inventory",
                principalColumn: "InventoryID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorefrontOrderAllocations_Locations_LocationID",
                table: "StorefrontOrderAllocations",
                column: "LocationID",
                principalTable: "Locations",
                principalColumn: "LocationID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorefrontOrderAllocations_Products_ProductID",
                table: "StorefrontOrderAllocations",
                column: "ProductID",
                principalTable: "Products",
                principalColumn: "ProductID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorefrontOrderAllocations_Warehouses_WarehouseID",
                table: "StorefrontOrderAllocations",
                column: "WarehouseID",
                principalTable: "Warehouses",
                principalColumn: "WarehouseID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorefrontOrders_Warehouses_AssignedWarehouseID",
                table: "StorefrontOrders",
                column: "AssignedWarehouseID",
                principalTable: "Warehouses",
                principalColumn: "WarehouseID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleID",
                table: "Users",
                column: "RoleID",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Warehouses_WarehouseID",
                table: "Users",
                column: "WarehouseID",
                principalTable: "Warehouses",
                principalColumn: "WarehouseID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
