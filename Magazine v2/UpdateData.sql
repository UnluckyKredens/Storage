USE [MagazineAPIDev];
GO

SET QUOTED_IDENTIFIER ON;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Początkowe Role
MERGE dbo.Roles AS target
    USING
    (
        VALUES
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000001'), N'Administrator'),
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), N'Kierownik'),
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), N'Pracownik')
    ) AS source (Id, Name)
        ON target.Id = source.Id
    WHEN MATCHED THEN
        UPDATE SET Name = source.Name
    WHEN NOT MATCHED THEN
        INSERT (Id, Name)
        VALUES (source.Id, source.Name);

-- Początkowe Uprawnienia
MERGE dbo.RolePermissions AS target
    USING
    (
        VALUES
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000001')), -- Kierownik: products.read
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000002')), -- Kierownik: products.manage
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000003')), -- Kierownik: inventory.read
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000004')), -- Kierownik: inventory.manage
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000005')), -- Kierownik: warehouses.read
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000007')), -- Kierownik: contractors.read
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000012')), -- Kierownik: stock-documents.manage
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000013')), -- Kierownik: stock-documents.approve
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000014')), -- Kierownik: stock-documents.read
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000015')), -- Kierownik: stock-documents.receive
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000016')), -- Kierownik: stock-shipments.create
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000001')), -- Pracownik: products.read
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000003')), -- Pracownik: inventory.read
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000015')), -- Pracownik: stock-documents.receive
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000016'))  -- Pracownik: stock-shipments.create
    ) AS source (RoleId, PermissionId)
        ON target.RoleId = source.RoleId
        AND target.PermissionId = source.PermissionId
    WHEN NOT MATCHED THEN
        INSERT (RoleId, PermissionId)
        VALUES (source.RoleId, source.PermissionId)
    WHEN NOT MATCHED BY SOURCE
        AND target.RoleId IN
        (
            CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'),
            CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003')
        ) THEN DELETE;

MERGE dbo.UnitsOfMeasure AS target
    USING
    (
        VALUES
            (N'Sztuka', N'szt.'),
            (N'Opakowanie', N'op.'),
            (N'Kilogram', N'kg'),
            (N'Litr', N'l'),
            (N'Metr', N'm')
    ) AS source (Name, Symbol)
        ON target.Symbol = source.Symbol
    WHEN MATCHED THEN
        UPDATE SET Name = source.Name
    WHEN NOT MATCHED THEN
        INSERT (UnitOfMeasureID, Name, Symbol)
        VALUES (NEWID(), source.Name, source.Symbol);