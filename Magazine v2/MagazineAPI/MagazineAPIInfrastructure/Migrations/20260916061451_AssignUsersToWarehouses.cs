using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagazineAPInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssignUsersToWarehouses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseID",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE dbo.Users
                SET WarehouseID = NULL
                WHERE RoleID = '10000000-0000-0000-0000-000000000001';

                DECLARE @DefaultWarehouseID uniqueidentifier =
                    (SELECT TOP (1) WarehouseID FROM dbo.Warehouses ORDER BY Name);

                IF @DefaultWarehouseID IS NULL
                   AND EXISTS (
                       SELECT 1 FROM dbo.Users
                       WHERE RoleID <> '10000000-0000-0000-0000-000000000001')
                    THROW 50010, 'Nie można przypisać użytkowników, ponieważ nie istnieje żaden magazyn.', 1;

                UPDATE dbo.Users
                SET WarehouseID = @DefaultWarehouseID
                WHERE RoleID <> '10000000-0000-0000-0000-000000000001';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Users_WarehouseID",
                table: "Users",
                column: "WarehouseID");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_WarehouseAssignment",
                table: "Users",
                sql: "[RoleID] = '10000000-0000-0000-0000-000000000001' OR [WarehouseID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Warehouses_WarehouseID",
                table: "Users",
                column: "WarehouseID",
                principalTable: "Warehouses",
                principalColumn: "WarehouseID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Warehouses_WarehouseID",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_WarehouseID",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_WarehouseAssignment",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WarehouseID",
                table: "Users");
        }
    }
}
