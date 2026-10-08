using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MagazineAPInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "Contractors",
                columns: table => new
                {
                    ContractorID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contractors", x => x.ContractorID);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitsOfMeasure",
                columns: table => new
                {
                    UnitOfMeasureID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitsOfMeasure", x => x.UnitOfMeasureID);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    WarehouseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.WarehouseID);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UnitOfMeasureID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MinimumQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false, defaultValue: 0m),
                    OptimumQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                    table.CheckConstraint("CK_Products_Prices", "[PurchasePrice] >= 0 AND [SalePrice] >= 0");
                    table.CheckConstraint("CK_Products_QuantityThresholds", "[MinimumQuantity] >= 0 AND ([OptimumQuantity] IS NULL OR [OptimumQuantity] >= [MinimumQuantity])");
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_UnitsOfMeasure_UnitOfMeasureID",
                        column: x => x.UnitOfMeasureID,
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "UnitOfMeasureID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentNumberSequences",
                columns: table => new
                {
                    DocumentNumberSequenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastNumber = table.Column<int>(type: "int", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentNumberSequences", x => x.DocumentNumberSequenceId);
                    table.ForeignKey(
                        name: "FK_DocumentNumberSequences_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    LocationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.LocationID);
                    table.ForeignKey(
                        name: "FK_Locations_Warehouses_WarehouseID",
                        column: x => x.WarehouseID,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StorefrontOrders",
                columns: table => new
                {
                    StorefrontOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AssignedWarehouseID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorefrontOrders", x => x.StorefrontOrderId);
                    table.CheckConstraint("CK_StorefrontOrders_Status", "[Status] IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_StorefrontOrders_Warehouses_AssignedWarehouseID",
                        column: x => x.AssignedWarehouseID,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Login = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    RoleID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.CheckConstraint("CK_Users_WarehouseAssignment", "[RoleID] = '10000000-0000-0000-0000-000000000001' OR [WarehouseID] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Warehouses_WarehouseID",
                        column: x => x.WarehouseID,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    InventoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReservedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    AvailableQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false, computedColumnSql: "[Quantity] - [ReservedQuantity]", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.InventoryID);
                    table.CheckConstraint("CK_Inventory_QuantityValues", "[Quantity] >= 0 AND [ReservedQuantity] >= 0 AND [ReservedQuantity] <= [Quantity]");
                    table.ForeignKey(
                        name: "FK_Inventory_Locations_LocationID",
                        column: x => x.LocationID,
                        principalTable: "Locations",
                        principalColumn: "LocationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inventory_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StorefrontOrderItems",
                columns: table => new
                {
                    StorefrontOrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorefrontOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorefrontOrderItems", x => x.StorefrontOrderItemId);
                    table.CheckConstraint("CK_StorefrontOrderItems_Quantity", "[Quantity] > 0");
                    table.CheckConstraint("CK_StorefrontOrderItems_UnitPrice", "[UnitPrice] >= 0");
                    table.ForeignKey(
                        name: "FK_StorefrontOrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StorefrontOrderItems_StorefrontOrders_StorefrontOrderId",
                        column: x => x.StorefrontOrderId,
                        principalTable: "StorefrontOrders",
                        principalColumn: "StorefrontOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BeforeValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceivedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaperDocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.PurchaseOrderId);
                    table.CheckConstraint("CK_PurchaseOrders_Status", "[Status] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Contractors_ContractorId",
                        column: x => x.ContractorId,
                        principalTable: "Contractors",
                        principalColumn: "ContractorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Users_ReceivedByUserId",
                        column: x => x.ReceivedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceivedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.ShipmentId);
                    table.CheckConstraint("CK_Shipments_DifferentWarehouses", "[SourceWarehouseId] <> [DestinationWarehouseId]");
                    table.CheckConstraint("CK_Shipments_Status", "[Status] IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_Shipments_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shipments_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shipments_Users_ReceivedByUserId",
                        column: x => x.ReceivedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shipments_Warehouses_DestinationWarehouseId",
                        column: x => x.DestinationWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shipments_Warehouses_SourceWarehouseId",
                        column: x => x.SourceWarehouseId,
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
                name: "StockMovements",
                columns: table => new
                {
                    StockMovementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    QuantityBefore = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    QuantityChange = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    QuantityAfter = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReservedQuantityBefore = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReservedQuantityChange = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReservedQuantityAfter = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.StockMovementId);
                    table.CheckConstraint("CK_StockMovements_Type", "[Type] IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12)");
                    table.ForeignKey(
                        name: "FK_StockMovements_Inventory_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventory",
                        principalColumn: "InventoryID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockMovements_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseID",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "StorefrontOrderAllocations",
                columns: table => new
                {
                    StorefrontOrderAllocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorefrontOrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ConsumedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorefrontOrderAllocations", x => x.StorefrontOrderAllocationId);
                    table.CheckConstraint("CK_StorefrontOrderAllocations_QuantityValues", "[Quantity] > 0 AND [ConsumedQuantity] >= 0 AND [ConsumedQuantity] <= [Quantity]");
                    table.ForeignKey(
                        name: "FK_StorefrontOrderAllocations_Inventory_InventoryID",
                        column: x => x.InventoryID,
                        principalTable: "Inventory",
                        principalColumn: "InventoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StorefrontOrderAllocations_Locations_LocationID",
                        column: x => x.LocationID,
                        principalTable: "Locations",
                        principalColumn: "LocationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StorefrontOrderAllocations_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StorefrontOrderAllocations_StorefrontOrderItems_StorefrontOrderItemId",
                        column: x => x.StorefrontOrderItemId,
                        principalTable: "StorefrontOrderItems",
                        principalColumn: "StorefrontOrderItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StorefrontOrderAllocations_Warehouses_WarehouseID",
                        column: x => x.WarehouseID,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderItems",
                columns: table => new
                {
                    PurchaseOrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderItems", x => x.PurchaseOrderItemId);
                    table.CheckConstraint("CK_PurchaseOrderItems_Quantity", "[Quantity] > 0");
                    table.CheckConstraint("CK_PurchaseOrderItems_UnitPrice", "[UnitPrice] >= 0");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "PurchaseOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentItems",
                columns: table => new
                {
                    ShipmentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentItems", x => x.ShipmentItemId);
                    table.CheckConstraint("CK_ShipmentItems_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_ShipmentItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShipmentItems_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "ShipmentId",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "products.read", "Wyświetlanie katalogu i szczegółów produktów.", "Podgląd produktów" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "products.manage", "Dodawanie, edycja i wycofywanie produktów.", "Zarządzanie produktami" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "inventory.read", "Wyświetlanie stanów i rezerwacji magazynowych.", "Podgląd stanów" },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "inventory.manage", "Bezpośrednia edycja stanów magazynowych.", "Zarządzanie stanami" },
                    { new Guid("20000000-0000-0000-0000-000000000005"), "warehouses.read", "Wyświetlanie magazynów oraz lokalizacji.", "Podgląd magazynów" },
                    { new Guid("20000000-0000-0000-0000-000000000006"), "warehouses.manage", "Dodawanie i edycja magazynów oraz lokalizacji.", "Zarządzanie magazynami" },
                    { new Guid("20000000-0000-0000-0000-000000000007"), "contractors.read", "Wyświetlanie dostawców i odbiorców.", "Podgląd kontrahentów" },
                    { new Guid("20000000-0000-0000-0000-000000000008"), "contractors.manage", "Dodawanie i edycja kontrahentów.", "Zarządzanie kontrahentami" },
                    { new Guid("20000000-0000-0000-0000-000000000009"), "users.read", "Wyświetlanie kont użytkowników.", "Podgląd użytkowników" },
                    { new Guid("20000000-0000-0000-0000-000000000010"), "users.manage", "Edycja i usuwanie kont innych niż administratorzy.", "Zarządzanie użytkownikami" },
                    { new Guid("20000000-0000-0000-0000-000000000011"), "dictionaries.manage", "Edycja kategorii i jednostek miary.", "Zarządzanie słownikami" },
                    { new Guid("20000000-0000-0000-0000-000000000012"), "shipments.read", "Wyświetlanie wysyłek między magazynami.", "Podgląd wysyłek" },
                    { new Guid("20000000-0000-0000-0000-000000000013"), "shipments.create", "Tworzenie wysyłek z magazynu pracownika.", "Tworzenie wysyłek" },
                    { new Guid("20000000-0000-0000-0000-000000000014"), "shipments.approve", "Akceptowanie wysyłek i przygotowanie WZ.", "Akceptacja wysyłek" },
                    { new Guid("20000000-0000-0000-0000-000000000015"), "purchase-orders.read", "Wyświetlanie zamówień do magazynu od dostawców.", "Podgląd zamówień zewnętrznych" },
                    { new Guid("20000000-0000-0000-0000-000000000016"), "purchase-orders.create", "Tworzenie zamówień do aktywnego magazynu.", "Tworzenie zamówień zewnętrznych" },
                    { new Guid("20000000-0000-0000-0000-000000000017"), "purchase-orders.approve", "Akceptowanie zamówień, faktur i dokumentów przyjęcia.", "Akceptacja zamówień zewnętrznych" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "Administrator" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "Kierownik" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "Pracownik" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000002"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000003"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000004"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000005"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000007"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000012"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000013"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000014"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000015"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000016"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000017"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000003") },
                    { new Guid("20000000-0000-0000-0000-000000000003"), new Guid("10000000-0000-0000-0000-000000000003") },
                    { new Guid("20000000-0000-0000-0000-000000000012"), new Guid("10000000-0000-0000-0000-000000000003") },
                    { new Guid("20000000-0000-0000-0000-000000000013"), new Guid("10000000-0000-0000-0000-000000000003") },
                    { new Guid("20000000-0000-0000-0000-000000000015"), new Guid("10000000-0000-0000-0000-000000000003") },
                    { new Guid("20000000-0000-0000-0000-000000000016"), new Guid("10000000-0000-0000-0000-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedOnUtc",
                table: "AuditLogs",
                column: "CreatedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentNumberSequences_WarehouseId_DocumentType_Year",
                table: "DocumentNumberSequences",
                columns: new[] { "WarehouseId", "DocumentType", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_LocationID",
                table: "Inventory",
                column: "LocationID");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_ProductID_LocationID",
                table: "Inventory",
                columns: new[] { "ProductID", "LocationID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_WarehouseID_LocationCode",
                table: "Locations",
                columns: new[] { "WarehouseID", "LocationCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryID",
                table: "Products",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitOfMeasureID",
                table: "Products",
                column: "UnitOfMeasureID");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_ProductId",
                table: "PurchaseOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId_ProductId",
                table: "PurchaseOrderItems",
                columns: new[] { "PurchaseOrderId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_ApprovedByUserId",
                table: "PurchaseOrders",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_ContractorId",
                table: "PurchaseOrders",
                column: "ContractorId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_CreatedByUserId",
                table: "PurchaseOrders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_Number",
                table: "PurchaseOrders",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_ReceivedByUserId",
                table: "PurchaseOrders",
                column: "ReceivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_Status_CreatedOnUtc",
                table: "PurchaseOrders",
                columns: new[] { "Status", "CreatedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_WarehouseId_CreatedOnUtc",
                table: "PurchaseOrders",
                columns: new[] { "WarehouseId", "CreatedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_ProductId",
                table: "ShipmentItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_ShipmentId_ProductId",
                table: "ShipmentItems",
                columns: new[] { "ShipmentId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ApprovedByUserId",
                table: "Shipments",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_CreatedByUserId",
                table: "Shipments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_DestinationWarehouseId_CreatedOnUtc",
                table: "Shipments",
                columns: new[] { "DestinationWarehouseId", "CreatedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Number",
                table: "Shipments",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ReceivedByUserId",
                table: "Shipments",
                column: "ReceivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_SourceWarehouseId_CreatedOnUtc",
                table: "Shipments",
                columns: new[] { "SourceWarehouseId", "CreatedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Status_CreatedOnUtc",
                table: "Shipments",
                columns: new[] { "Status", "CreatedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_CreatedByUserId",
                table: "StockMovements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_CreatedOnUtc",
                table: "StockMovements",
                column: "CreatedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_InventoryId",
                table: "StockMovements",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_LocationId",
                table: "StockMovements",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductId_CreatedOnUtc",
                table: "StockMovements",
                columns: new[] { "ProductId", "CreatedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_SourceType_SourceId",
                table: "StockMovements",
                columns: new[] { "SourceType", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_WarehouseId_CreatedOnUtc",
                table: "StockMovements",
                columns: new[] { "WarehouseId", "CreatedOnUtc" });

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
                name: "IX_StorefrontOrderAllocations_InventoryID",
                table: "StorefrontOrderAllocations",
                column: "InventoryID");

            migrationBuilder.CreateIndex(
                name: "IX_StorefrontOrderAllocations_LocationID",
                table: "StorefrontOrderAllocations",
                column: "LocationID");

            migrationBuilder.CreateIndex(
                name: "IX_StorefrontOrderAllocations_ProductID",
                table: "StorefrontOrderAllocations",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_StorefrontOrderAllocations_StorefrontOrderItemId",
                table: "StorefrontOrderAllocations",
                column: "StorefrontOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StorefrontOrderAllocations_WarehouseID_ProductID",
                table: "StorefrontOrderAllocations",
                columns: new[] { "WarehouseID", "ProductID" });

            migrationBuilder.CreateIndex(
                name: "IX_StorefrontOrderItems_ProductId",
                table: "StorefrontOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StorefrontOrderItems_StorefrontOrderId",
                table: "StorefrontOrderItems",
                column: "StorefrontOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_StorefrontOrders_AssignedWarehouseID_Status",
                table: "StorefrontOrders",
                columns: new[] { "AssignedWarehouseID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StorefrontOrders_CreatedOnUtc",
                table: "StorefrontOrders",
                column: "CreatedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_StorefrontOrders_Number",
                table: "StorefrontOrders",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Login",
                table: "Users",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleID",
                table: "Users",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_WarehouseID",
                table: "Users",
                column: "WarehouseID");

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
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DocumentNumberSequences");

            migrationBuilder.DropTable(
                name: "PurchaseOrderItems");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "ShipmentItems");

            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "StockReservations");

            migrationBuilder.DropTable(
                name: "StorefrontOrderAllocations");

            migrationBuilder.DropTable(
                name: "WarehouseOperationItems");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "StorefrontOrderItems");

            migrationBuilder.DropTable(
                name: "WarehouseOperations");

            migrationBuilder.DropTable(
                name: "Contractors");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "StorefrontOrders");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "UnitsOfMeasure");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Warehouses");
        }
    }
}
