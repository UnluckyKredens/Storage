using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MagazineAPInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShipmentReceivingWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StockDocuments_Status",
                table: "StockDocuments");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("20000000-0000-0000-0000-000000000012"), new Guid("10000000-0000-0000-0000-000000000003") });

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAtUtc",
                table: "StockDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceivedByUserId",
                table: "StockDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000014"), "stock-documents.read", "Wyświetlanie list dokumentów i ich historii.", "Podgląd dokumentów PZ i WZ" },
                    { new Guid("20000000-0000-0000-0000-000000000015"), "stock-documents.receive", "Skanowanie kodu, podgląd zawartości i przyjmowanie przesyłek PZ.", "Odbiór przesyłek" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000014"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000015"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000015"), new Guid("10000000-0000-0000-0000-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockDocuments_ReceivedByUserId",
                table: "StockDocuments",
                column: "ReceivedByUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockDocuments_Status",
                table: "StockDocuments",
                sql: "[Status] IN (1, 2, 3)");

            migrationBuilder.AddForeignKey(
                name: "FK_StockDocuments_Users_ReceivedByUserId",
                table: "StockDocuments",
                column: "ReceivedByUserId",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockDocuments_Users_ReceivedByUserId",
                table: "StockDocuments");

            migrationBuilder.DropIndex(
                name: "IX_StockDocuments_ReceivedByUserId",
                table: "StockDocuments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockDocuments_Status",
                table: "StockDocuments");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("20000000-0000-0000-0000-000000000014"), new Guid("10000000-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("20000000-0000-0000-0000-000000000015"), new Guid("10000000-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("20000000-0000-0000-0000-000000000015"), new Guid("10000000-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000015"));

            migrationBuilder.DropColumn(
                name: "ReceivedAtUtc",
                table: "StockDocuments");

            migrationBuilder.DropColumn(
                name: "ReceivedByUserId",
                table: "StockDocuments");

            migrationBuilder.Sql(
                "UPDATE [StockDocuments] SET [Status] = 1 WHERE [Status] = 3");

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { new Guid("20000000-0000-0000-0000-000000000012"), new Guid("10000000-0000-0000-0000-000000000003") });

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockDocuments_Status",
                table: "StockDocuments",
                sql: "[Status] IN (1, 2)");
        }
    }
}
