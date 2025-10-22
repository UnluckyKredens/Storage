using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagazineAPInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OperationalIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Users_CreatedByUserId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId_ProductId",
                table: "PurchaseOrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "WarehouseOperations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "SecurityVersion",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "StorefrontOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresOnUtc",
                table: "StorefrontOrders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "StorefrontOrders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestFingerprint",
                table: "StorefrontOrders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "StorefrontOrders",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "TrackingToken",
                table: "StorefrontOrders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "SourceNumber",
                table: "StockReservations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SourceNumber",
                table: "StockMovements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "StockMovements",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Shipments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Shipments",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "PurchaseOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PurchaseOrders",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Inventory",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "HistoryOutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryOutboxMessages", x => x.Id);
                });

            // Preserve existing orders: new secrets are independent of their public numbers.
            migrationBuilder.Sql("UPDATE StorefrontOrders SET TrackingToken = LOWER(CONVERT(varchar(64), CRYPT_GEN_RANDOM(32), 2)), ExpiresOnUtc = DATEADD(hour, 24, SYSUTCDATETIME())");
            migrationBuilder.Sql("UPDATE Products SET Barcode = NULL WHERE LTRIM(RTRIM(Barcode)) = ''");
            migrationBuilder.Sql("IF EXISTS (SELECT Barcode FROM Products WHERE Barcode IS NOT NULL GROUP BY Barcode HAVING COUNT(*) > 1) THROW 51000, 'Resolve duplicate product barcodes before applying OperationalIntegrity.', 1;");

            migrationBuilder.CreateIndex(
                name: "IX_StorefrontOrders_IdempotencyKey",
                table: "StorefrontOrders",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StorefrontOrders_Status_ExpiresOnUtc",
                table: "StorefrontOrders",
                columns: new[] { "Status", "ExpiresOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StorefrontOrders_TrackingToken",
                table: "StorefrontOrders",
                column: "TrackingToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId_ProductId_UnitPrice",
                table: "PurchaseOrderItems",
                columns: new[] { "PurchaseOrderId", "ProductId", "UnitPrice" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryOutboxMessages_ProcessedOnUtc_CreatedOnUtc",
                table: "HistoryOutboxMessages",
                columns: new[] { "ProcessedOnUtc", "CreatedOnUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Users_CreatedByUserId",
                table: "StockMovements",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Users_CreatedByUserId",
                table: "StockMovements");

            migrationBuilder.DropTable(
                name: "HistoryOutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_StorefrontOrders_IdempotencyKey",
                table: "StorefrontOrders");

            migrationBuilder.DropIndex(
                name: "IX_StorefrontOrders_Status_ExpiresOnUtc",
                table: "StorefrontOrders");

            migrationBuilder.DropIndex(
                name: "IX_StorefrontOrders_TrackingToken",
                table: "StorefrontOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId_ProductId_UnitPrice",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Products_Barcode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SecurityVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExpiresOnUtc",
                table: "StorefrontOrders");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "StorefrontOrders");

            migrationBuilder.DropColumn(
                name: "RequestFingerprint",
                table: "StorefrontOrders");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "StorefrontOrders");

            migrationBuilder.DropColumn(
                name: "TrackingToken",
                table: "StorefrontOrders");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Inventory");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "WarehouseOperations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "StorefrontOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "SourceNumber",
                table: "StockReservations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SourceNumber",
                table: "StockMovements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "StockMovements",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Shipments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "PurchaseOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId_ProductId",
                table: "PurchaseOrderItems",
                columns: new[] { "PurchaseOrderId", "ProductId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Users_CreatedByUserId",
                table: "StockMovements",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
