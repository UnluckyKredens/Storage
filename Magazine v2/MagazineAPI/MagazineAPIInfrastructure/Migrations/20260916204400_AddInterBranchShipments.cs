using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MagazineAPInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInterBranchShipments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ContractorId",
                table: "StockDocuments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "DestinationWarehouseId",
                table: "StockDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Description", "Name" },
                values: new object[] { new Guid("20000000-0000-0000-0000-000000000016"), "stock-shipments.create", "Tworzenie szkiców WZ kierowanych do innego oddziału.", "Tworzenie wysyłek międzyoddziałowych" });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000016"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000016"), new Guid("10000000-0000-0000-0000-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockDocuments_DestinationWarehouseId",
                table: "StockDocuments",
                column: "DestinationWarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockDocuments_Warehouses_DestinationWarehouseId",
                table: "StockDocuments",
                column: "DestinationWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "WarehouseID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockDocuments_Warehouses_DestinationWarehouseId",
                table: "StockDocuments");

            migrationBuilder.DropIndex(
                name: "IX_StockDocuments_DestinationWarehouseId",
                table: "StockDocuments");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("20000000-0000-0000-0000-000000000016"), new Guid("10000000-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("20000000-0000-0000-0000-000000000016"), new Guid("10000000-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000016"));

            migrationBuilder.DropColumn(
                name: "DestinationWarehouseId",
                table: "StockDocuments");

            migrationBuilder.Sql("""
                DELETE items
                FROM [StockDocumentItems] items
                INNER JOIN [StockDocuments] documents ON documents.[Id] = items.[StockDocumentId]
                WHERE documents.[ContractorId] IS NULL;

                DELETE FROM [StockDocuments] WHERE [ContractorId] IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "ContractorId",
                table: "StockDocuments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
