using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MagazineAPInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRolePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "products.read", "Wyświetlanie katalogu i szczegółów produktów.", "Podgląd produktów" },
                    { 2, "products.manage", "Dodawanie, edycja i wycofywanie produktów.", "Zarządzanie produktami" },
                    { 3, "inventory.read", "Wyświetlanie stanów i rezerwacji magazynowych.", "Podgląd stanów" },
                    { 4, "inventory.manage", "Przyjęcia, wydania, przesunięcia i korekty stanów.", "Zarządzanie stanami" },
                    { 5, "warehouses.read", "Wyświetlanie magazynów oraz lokalizacji.", "Podgląd magazynów" },
                    { 6, "warehouses.manage", "Dodawanie i edycja magazynów oraz lokalizacji.", "Zarządzanie magazynami" },
                    { 7, "contractors.read", "Wyświetlanie dostawców i odbiorców.", "Podgląd kontrahentów" },
                    { 8, "contractors.manage", "Dodawanie i edycja kontrahentów.", "Zarządzanie kontrahentami" },
                    { 9, "users.read", "Wyświetlanie kont użytkowników.", "Podgląd użytkowników" },
                    { 10, "users.manage", "Edycja i usuwanie kont innych niż administratorzy.", "Zarządzanie użytkownikami" },
                    { 11, "dictionaries.manage", "Edycja kategorii i jednostek miary.", "Zarządzanie słownikami" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 2 },
                    { 2, 2 },
                    { 3, 2 },
                    { 4, 2 },
                    { 5, 2 },
                    { 7, 2 },
                    { 1, 3 },
                    { 3, 3 },
                    { 4, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Permissions");
        }
    }
}
