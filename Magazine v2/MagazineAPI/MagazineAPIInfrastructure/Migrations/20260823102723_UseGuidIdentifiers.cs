using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagazineAPInfrastructure.Migrations;

/// <inheritdoc />
public partial class UseGuidIdentifiers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE dbo.Roles ADD NewId uniqueidentifier NULL;
            ALTER TABLE dbo.Permissions ADD NewId uniqueidentifier NULL;
            ALTER TABLE dbo.Warehouses ADD NewWarehouseID uniqueidentifier NULL;
            ALTER TABLE dbo.UnitsOfMeasure ADD NewUnitOfMeasureID uniqueidentifier NULL;
            ALTER TABLE dbo.Categories ADD NewCategoryID uniqueidentifier NULL;
            ALTER TABLE dbo.Contractors ADD NewContractorID uniqueidentifier NULL;
            ALTER TABLE dbo.Products ADD NewProductID uniqueidentifier NULL;
            ALTER TABLE dbo.Products ADD NewUnitOfMeasureID uniqueidentifier NULL;
            ALTER TABLE dbo.Products ADD NewCategoryID uniqueidentifier NULL;
            ALTER TABLE dbo.Locations ADD NewLocationID uniqueidentifier NULL;
            ALTER TABLE dbo.Locations ADD NewWarehouseID uniqueidentifier NULL;
            ALTER TABLE dbo.Inventory ADD NewInventoryID uniqueidentifier NULL;
            ALTER TABLE dbo.Inventory ADD NewProductID uniqueidentifier NULL;
            ALTER TABLE dbo.Inventory ADD NewLocationID uniqueidentifier NULL;
            ALTER TABLE dbo.Users ADD NewRoleID uniqueidentifier NULL;
            ALTER TABLE dbo.RolePermissions ADD NewRoleId uniqueidentifier NULL;
            ALTER TABLE dbo.RolePermissions ADD NewPermissionId uniqueidentifier NULL;
            """);

        migrationBuilder.Sql(
            """
            EXEC(N'
            UPDATE dbo.Roles
            SET NewId = CASE Id
                WHEN 1 THEN CONVERT(uniqueidentifier, ''10000000-0000-0000-0000-000000000001'')
                WHEN 2 THEN CONVERT(uniqueidentifier, ''10000000-0000-0000-0000-000000000002'')
                WHEN 3 THEN CONVERT(uniqueidentifier, ''10000000-0000-0000-0000-000000000003'')
                ELSE NEWID()
            END;

            UPDATE dbo.Permissions
            SET NewId = CASE Id
                WHEN 1 THEN CONVERT(uniqueidentifier, ''20000000-0000-0000-0000-000000000001'')
                WHEN 2 THEN CONVERT(uniqueidentifier, ''20000000-0000-0000-0000-000000000002'')
                WHEN 3 THEN CONVERT(uniqueidentifier, ''20000000-0000-0000-0000-000000000003'')
                WHEN 4 THEN CONVERT(uniqueidentifier, ''20000000-0000-0000-0000-000000000004'')
                WHEN 5 THEN CONVERT(uniqueidentifier, ''20000000-0000-0000-0000-000000000005'')
                WHEN 6 THEN CONVERT(uniqueidentifier, ''20000000-0000-0000-0000-000000000006'')
                WHEN 7 THEN CONVERT(uniqueidentifier, ''20000000-0000-0000-0000-000000000007'')
                WHEN 8 THEN CONVERT(uniqueidentifier, ''20000000-0000-0000-0000-000000000008'')
                WHEN 9 THEN CONVERT(uniqueidentifier, ''20000000-0000-0000-0000-000000000009'')
                WHEN 10 THEN CONVERT(uniqueidentifier, ''20000000-0000-0000-0000-000000000010'')
                WHEN 11 THEN CONVERT(uniqueidentifier, ''20000000-0000-0000-0000-000000000011'')
                ELSE NEWID()
            END;

            UPDATE dbo.Warehouses SET NewWarehouseID = NEWID();
            UPDATE dbo.UnitsOfMeasure SET NewUnitOfMeasureID = NEWID();
            UPDATE dbo.Categories SET NewCategoryID = NEWID();
            UPDATE dbo.Contractors SET NewContractorID = NEWID();
            UPDATE dbo.Products SET NewProductID = NEWID();
            UPDATE dbo.Locations SET NewLocationID = NEWID();
            UPDATE dbo.Inventory SET NewInventoryID = NEWID();

            UPDATE users
            SET NewRoleID = roles.NewId
            FROM dbo.Users AS users
            INNER JOIN dbo.Roles AS roles ON roles.Id = users.RoleID;

            UPDATE assignments
            SET NewRoleId = roles.NewId,
                NewPermissionId = permissions.NewId
            FROM dbo.RolePermissions AS assignments
            INNER JOIN dbo.Roles AS roles ON roles.Id = assignments.RoleId
            INNER JOIN dbo.Permissions AS permissions ON permissions.Id = assignments.PermissionId;

            UPDATE products
            SET NewUnitOfMeasureID = units.NewUnitOfMeasureID,
                NewCategoryID = categories.NewCategoryID
            FROM dbo.Products AS products
            INNER JOIN dbo.UnitsOfMeasure AS units
                ON units.UnitOfMeasureID = products.UnitOfMeasureID
            INNER JOIN dbo.Categories AS categories
                ON categories.CategoryID = products.CategoryID;

            UPDATE locations
            SET NewWarehouseID = warehouses.NewWarehouseID
            FROM dbo.Locations AS locations
            INNER JOIN dbo.Warehouses AS warehouses
                ON warehouses.WarehouseID = locations.WarehouseID;

            UPDATE inventory
            SET NewProductID = products.NewProductID,
                NewLocationID = locations.NewLocationID
            FROM dbo.Inventory AS inventory
            INNER JOIN dbo.Products AS products ON products.ProductID = inventory.ProductID
            INNER JOIN dbo.Locations AS locations ON locations.LocationID = inventory.LocationID;

            IF EXISTS (SELECT 1 FROM dbo.Users WHERE NewRoleID IS NULL)
                THROW 50001, ''Nie udało się odwzorować wszystkich Users.RoleID na GUID.'', 1;
            IF EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE NewRoleId IS NULL OR NewPermissionId IS NULL)
                THROW 50002, ''Nie udało się odwzorować wszystkich RolePermissions na GUID.'', 1;
            IF EXISTS (SELECT 1 FROM dbo.Products WHERE NewUnitOfMeasureID IS NULL OR NewCategoryID IS NULL)
                THROW 50003, ''Nie udało się odwzorować wszystkich kluczy obcych produktów na GUID.'', 1;
            IF EXISTS (SELECT 1 FROM dbo.Locations WHERE NewWarehouseID IS NULL)
                THROW 50004, ''Nie udało się odwzorować wszystkich Locations.WarehouseID na GUID.'', 1;
            IF EXISTS (SELECT 1 FROM dbo.Inventory WHERE NewProductID IS NULL OR NewLocationID IS NULL)
                THROW 50005, ''Nie udało się odwzorować wszystkich kluczy obcych stanów na GUID.'', 1;
            ');
            """);

        migrationBuilder.Sql(
            """
            ALTER TABLE dbo.Users DROP CONSTRAINT FK_Users_Roles_RoleID;
            ALTER TABLE dbo.RolePermissions DROP CONSTRAINT FK_RolePermissions_Permissions_PermissionId;
            ALTER TABLE dbo.RolePermissions DROP CONSTRAINT FK_RolePermissions_Roles_RoleId;
            ALTER TABLE dbo.Products DROP CONSTRAINT FK_Products_Categories_CategoryID;
            ALTER TABLE dbo.Products DROP CONSTRAINT FK_Products_UnitsOfMeasure_UnitOfMeasureID;
            ALTER TABLE dbo.Locations DROP CONSTRAINT FK_Locations_Warehouses_WarehouseID;
            ALTER TABLE dbo.Inventory DROP CONSTRAINT FK_Inventory_Locations_LocationID;
            ALTER TABLE dbo.Inventory DROP CONSTRAINT FK_Inventory_Products_ProductID;

            DROP INDEX IX_Users_RoleID ON dbo.Users;
            DROP INDEX IX_RolePermissions_PermissionId ON dbo.RolePermissions;
            DROP INDEX IX_Products_CategoryID ON dbo.Products;
            DROP INDEX IX_Products_UnitOfMeasureID ON dbo.Products;
            DROP INDEX IX_Locations_WarehouseID_LocationCode ON dbo.Locations;
            DROP INDEX IX_Inventory_LocationID ON dbo.Inventory;
            DROP INDEX IX_Inventory_ProductID_LocationID ON dbo.Inventory;

            ALTER TABLE dbo.RolePermissions DROP CONSTRAINT PK_RolePermissions;
            ALTER TABLE dbo.Roles DROP CONSTRAINT PK_Roles;
            ALTER TABLE dbo.Permissions DROP CONSTRAINT PK_Permissions;
            ALTER TABLE dbo.Warehouses DROP CONSTRAINT PK_Warehouses;
            ALTER TABLE dbo.UnitsOfMeasure DROP CONSTRAINT PK_UnitsOfMeasure;
            ALTER TABLE dbo.Categories DROP CONSTRAINT PK_Categories;
            ALTER TABLE dbo.Contractors DROP CONSTRAINT PK_Contractors;
            ALTER TABLE dbo.Products DROP CONSTRAINT PK_Products;
            ALTER TABLE dbo.Locations DROP CONSTRAINT PK_Locations;
            ALTER TABLE dbo.Inventory DROP CONSTRAINT PK_Inventory;

            ALTER TABLE dbo.Users DROP COLUMN RoleID;
            ALTER TABLE dbo.RolePermissions DROP COLUMN RoleId, PermissionId;
            ALTER TABLE dbo.Roles DROP COLUMN Id;
            ALTER TABLE dbo.Permissions DROP COLUMN Id;
            ALTER TABLE dbo.Products DROP COLUMN ProductID, UnitOfMeasureID, CategoryID;
            ALTER TABLE dbo.Locations DROP COLUMN LocationID, WarehouseID;
            ALTER TABLE dbo.Inventory DROP COLUMN InventoryID, ProductID, LocationID;
            ALTER TABLE dbo.Warehouses DROP COLUMN WarehouseID;
            ALTER TABLE dbo.UnitsOfMeasure DROP COLUMN UnitOfMeasureID;
            ALTER TABLE dbo.Categories DROP COLUMN CategoryID;
            ALTER TABLE dbo.Contractors DROP COLUMN ContractorID;
            """);

        migrationBuilder.Sql(
            """
            EXEC sp_rename N'dbo.Roles.NewId', N'Id', N'COLUMN';
            EXEC sp_rename N'dbo.Permissions.NewId', N'Id', N'COLUMN';
            EXEC sp_rename N'dbo.Warehouses.NewWarehouseID', N'WarehouseID', N'COLUMN';
            EXEC sp_rename N'dbo.UnitsOfMeasure.NewUnitOfMeasureID', N'UnitOfMeasureID', N'COLUMN';
            EXEC sp_rename N'dbo.Categories.NewCategoryID', N'CategoryID', N'COLUMN';
            EXEC sp_rename N'dbo.Contractors.NewContractorID', N'ContractorID', N'COLUMN';
            EXEC sp_rename N'dbo.Products.NewProductID', N'ProductID', N'COLUMN';
            EXEC sp_rename N'dbo.Products.NewUnitOfMeasureID', N'UnitOfMeasureID', N'COLUMN';
            EXEC sp_rename N'dbo.Products.NewCategoryID', N'CategoryID', N'COLUMN';
            EXEC sp_rename N'dbo.Locations.NewLocationID', N'LocationID', N'COLUMN';
            EXEC sp_rename N'dbo.Locations.NewWarehouseID', N'WarehouseID', N'COLUMN';
            EXEC sp_rename N'dbo.Inventory.NewInventoryID', N'InventoryID', N'COLUMN';
            EXEC sp_rename N'dbo.Inventory.NewProductID', N'ProductID', N'COLUMN';
            EXEC sp_rename N'dbo.Inventory.NewLocationID', N'LocationID', N'COLUMN';
            EXEC sp_rename N'dbo.Users.NewRoleID', N'RoleID', N'COLUMN';
            EXEC sp_rename N'dbo.RolePermissions.NewRoleId', N'RoleId', N'COLUMN';
            EXEC sp_rename N'dbo.RolePermissions.NewPermissionId', N'PermissionId', N'COLUMN';
            """);

        migrationBuilder.Sql(
            """
            EXEC(N'
            ALTER TABLE dbo.Roles ALTER COLUMN Id uniqueidentifier NOT NULL;
            ALTER TABLE dbo.Permissions ALTER COLUMN Id uniqueidentifier NOT NULL;
            ALTER TABLE dbo.Warehouses ALTER COLUMN WarehouseID uniqueidentifier NOT NULL;
            ALTER TABLE dbo.UnitsOfMeasure ALTER COLUMN UnitOfMeasureID uniqueidentifier NOT NULL;
            ALTER TABLE dbo.Categories ALTER COLUMN CategoryID uniqueidentifier NOT NULL;
            ALTER TABLE dbo.Contractors ALTER COLUMN ContractorID uniqueidentifier NOT NULL;
            ALTER TABLE dbo.Products ALTER COLUMN ProductID uniqueidentifier NOT NULL;
            ALTER TABLE dbo.Products ALTER COLUMN UnitOfMeasureID uniqueidentifier NOT NULL;
            ALTER TABLE dbo.Products ALTER COLUMN CategoryID uniqueidentifier NOT NULL;
            ALTER TABLE dbo.Locations ALTER COLUMN LocationID uniqueidentifier NOT NULL;
            ALTER TABLE dbo.Locations ALTER COLUMN WarehouseID uniqueidentifier NOT NULL;
            ALTER TABLE dbo.Inventory ALTER COLUMN InventoryID uniqueidentifier NOT NULL;
            ALTER TABLE dbo.Inventory ALTER COLUMN ProductID uniqueidentifier NOT NULL;
            ALTER TABLE dbo.Inventory ALTER COLUMN LocationID uniqueidentifier NOT NULL;
            ALTER TABLE dbo.Users ALTER COLUMN RoleID uniqueidentifier NOT NULL;
            ALTER TABLE dbo.RolePermissions ALTER COLUMN RoleId uniqueidentifier NOT NULL;
            ALTER TABLE dbo.RolePermissions ALTER COLUMN PermissionId uniqueidentifier NOT NULL;
            ');

            EXEC(N'
            ALTER TABLE dbo.Roles ADD CONSTRAINT PK_Roles PRIMARY KEY (Id);
            ALTER TABLE dbo.Permissions ADD CONSTRAINT PK_Permissions PRIMARY KEY (Id);
            ALTER TABLE dbo.Warehouses ADD CONSTRAINT PK_Warehouses PRIMARY KEY (WarehouseID);
            ALTER TABLE dbo.UnitsOfMeasure ADD CONSTRAINT PK_UnitsOfMeasure PRIMARY KEY (UnitOfMeasureID);
            ALTER TABLE dbo.Categories ADD CONSTRAINT PK_Categories PRIMARY KEY (CategoryID);
            ALTER TABLE dbo.Contractors ADD CONSTRAINT PK_Contractors PRIMARY KEY (ContractorID);
            ALTER TABLE dbo.Products ADD CONSTRAINT PK_Products PRIMARY KEY (ProductID);
            ALTER TABLE dbo.Locations ADD CONSTRAINT PK_Locations PRIMARY KEY (LocationID);
            ALTER TABLE dbo.Inventory ADD CONSTRAINT PK_Inventory PRIMARY KEY (InventoryID);
            ALTER TABLE dbo.RolePermissions ADD CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId);
            ');

            EXEC(N'
            CREATE INDEX IX_Users_RoleID ON dbo.Users (RoleID);
            CREATE INDEX IX_RolePermissions_PermissionId ON dbo.RolePermissions (PermissionId);
            CREATE INDEX IX_Products_CategoryID ON dbo.Products (CategoryID);
            CREATE INDEX IX_Products_UnitOfMeasureID ON dbo.Products (UnitOfMeasureID);
            CREATE UNIQUE INDEX IX_Locations_WarehouseID_LocationCode
                ON dbo.Locations (WarehouseID, LocationCode);
            CREATE INDEX IX_Inventory_LocationID ON dbo.Inventory (LocationID);
            CREATE UNIQUE INDEX IX_Inventory_ProductID_LocationID
                ON dbo.Inventory (ProductID, LocationID);
            ');

            EXEC(N'
            ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_Roles_RoleID
                FOREIGN KEY (RoleID) REFERENCES dbo.Roles (Id) ON DELETE NO ACTION;
            ALTER TABLE dbo.RolePermissions ADD CONSTRAINT FK_RolePermissions_Permissions_PermissionId
                FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions (Id) ON DELETE CASCADE;
            ALTER TABLE dbo.RolePermissions ADD CONSTRAINT FK_RolePermissions_Roles_RoleId
                FOREIGN KEY (RoleId) REFERENCES dbo.Roles (Id) ON DELETE CASCADE;
            ALTER TABLE dbo.Products ADD CONSTRAINT FK_Products_Categories_CategoryID
                FOREIGN KEY (CategoryID) REFERENCES dbo.Categories (CategoryID) ON DELETE NO ACTION;
            ALTER TABLE dbo.Products ADD CONSTRAINT FK_Products_UnitsOfMeasure_UnitOfMeasureID
                FOREIGN KEY (UnitOfMeasureID) REFERENCES dbo.UnitsOfMeasure (UnitOfMeasureID) ON DELETE NO ACTION;
            ALTER TABLE dbo.Locations ADD CONSTRAINT FK_Locations_Warehouses_WarehouseID
                FOREIGN KEY (WarehouseID) REFERENCES dbo.Warehouses (WarehouseID) ON DELETE NO ACTION;
            ALTER TABLE dbo.Inventory ADD CONSTRAINT FK_Inventory_Locations_LocationID
                FOREIGN KEY (LocationID) REFERENCES dbo.Locations (LocationID) ON DELETE NO ACTION;
            ALTER TABLE dbo.Inventory ADD CONSTRAINT FK_Inventory_Products_ProductID
                FOREIGN KEY (ProductID) REFERENCES dbo.Products (ProductID) ON DELETE NO ACTION;
            ');
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "Migracja identyfikatorów GUID jest nieodwracalna bez utraty mapowania danych. " +
            "Przywróć bazę z kopii zapasowej, jeśli wymagany jest rollback.");
    }
}
