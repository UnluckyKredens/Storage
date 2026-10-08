USE [MagazineAPIDev];
GO

SET QUOTED_IDENTIFIER ON;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

-- Aktualne role systemowe
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

MERGE dbo.Permissions AS target
USING
(
    VALUES
        (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000001'), N'products.read',       N'Podgląd produktów',         N'Wyświetlanie katalogu i szczegółów produktów.'),
        (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000002'), N'products.manage',     N'Zarządzanie produktami',    N'Dodawanie, edycja i wycofywanie produktów.'),
        (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000003'), N'inventory.read',      N'Podgląd stanów',            N'Wyświetlanie stanów i rezerwacji magazynowych.'),
        (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000004'), N'inventory.manage',    N'Zarządzanie stanami',       N'Bezpośrednia edycja stanów magazynowych.'),
        (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000005'), N'warehouses.read',     N'Podgląd magazynów',         N'Wyświetlanie magazynów oraz lokalizacji.'),
        (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000006'), N'warehouses.manage',   N'Zarządzanie magazynami',    N'Dodawanie i edycja magazynów oraz lokalizacji.'),
        (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000007'), N'contractors.read',    N'Podgląd kontrahentów',      N'Wyświetlanie dostawców i odbiorców.'),
        (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000008'), N'contractors.manage',  N'Zarządzanie kontrahentami', N'Dodawanie i edycja kontrahentów.'),
        (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000009'), N'users.read',          N'Podgląd użytkowników',      N'Wyświetlanie kont użytkowników.'),
        (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000010'), N'users.manage',        N'Zarządzanie użytkownikami', N'Edycja i usuwanie kont innych niż administratorzy.'),
        (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000011'), N'dictionaries.manage', N'Zarządzanie słownikami',    N'Edycja kategorii i jednostek miary.'),
        (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000012'), N'shipments.read',      N'Podgląd wysyłek',           N'Wyświetlanie wysyłek między magazynami.'),
        (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000013'), N'shipments.create',    N'Tworzenie wysyłek',         N'Tworzenie wysyłek z magazynu pracownika.'),
        (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000014'), N'shipments.approve',   N'Akceptacja wysyłek',        N'Akceptowanie wysyłek i przygotowanie WZ.')
) AS source (Id, Code, Name, Description)
    ON target.Id = source.Id
WHEN MATCHED THEN
    UPDATE SET
        Code = source.Code,
        Name = source.Name,
        Description = source.Description
WHEN NOT MATCHED THEN
    INSERT (Id, Code, Name, Description)
    VALUES (source.Id, source.Code, source.Name, source.Description);

MERGE dbo.RolePermissions AS target
USING
(
    VALUES
        -- Kierownik
        (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000001')), -- products.read
        (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000002')), -- products.manage
        (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000003')), -- inventory.read
        (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000004')), -- inventory.manage
        (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000005')), -- warehouses.read
        (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000007')), -- contractors.read
        (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000012')), -- shipments.read
        (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000013')), -- shipments.create
        (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000014')), -- shipments.approve

        -- Pracownik
        (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000001')), -- products.read
        (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000003')), -- inventory.read
        (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000012')), -- shipments.read
        (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000013'))  -- shipments.create
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

-- Podstawowe jednostki miary
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

-- Kategoria bazowa używana przez WarehouseBota
MERGE dbo.Categories AS target
USING
(
    VALUES
        (N'Sprzet IT', N'Kategoria utworzona automatycznie dla danych demonstracyjnych i WarehouseBota.')
) AS source (Name, Description)
    ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Description = source.Description
WHEN NOT MATCHED THEN
    INSERT (CategoryID, Name, Description)
    VALUES (NEWID(), source.Name, source.Description);

-- Bazowe oddziały zgodne z WarehouseBotem
MERGE dbo.Warehouses AS target
USING
(
    VALUES
        (N'Warszawa - Centrum Dystrybucyjne', N'ul. Logistyczna 1, 05-090 Sekocin Stary', N'Oddzial centralny utworzony dla danych demonstracyjnych.'),
        (N'Poznan - Oddzial Zachod', N'ul. Magazynowa 24, 62-080 Tarnowo Podgorne', N'Oddzial zachodni utworzony dla danych demonstracyjnych.'),
        (N'Krakow - Oddzial Poludnie', N'ul. Przemyslowa 42, 32-085 Modlniczka', N'Oddzial poludniowy utworzony dla danych demonstracyjnych.'),
        (N'Gdansk - Oddzial Polnoc', N'ul. Kontenerowa 19, 80-601 Gdansk', N'Oddzial polnocny utworzony dla danych demonstracyjnych.')
) AS source (Name, Address, Description)
    ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET
        Address = source.Address,
        Description = source.Description
WHEN NOT MATCHED THEN
    INSERT (WarehouseID, Name, Address, Description)
    VALUES (NEWID(), source.Name, source.Address, source.Description);

-- Bazowe lokalizacje w każdym oddziale zgodne z WarehouseBotem
MERGE dbo.Locations AS target
USING
(
    SELECT
        warehouse.WarehouseID,
        location.LocationCode,
        location.Description
    FROM dbo.Warehouses AS warehouse
    CROSS JOIN
    (
        VALUES
            (N'REC-01', N'Przyjecia i kontrola dostaw'),
            (N'A-01', N'Regaly glowne'),
            (N'B-01', N'Podzespoly i siec'),
            (N'PICK-01', N'Kompletacja'),
            (N'RET-01', N'Zwroty i reklamacje')
    ) AS location (LocationCode, Description)
    WHERE warehouse.Name IN
    (
        N'Warszawa - Centrum Dystrybucyjne',
        N'Poznan - Oddzial Zachod',
        N'Krakow - Oddzial Poludnie',
        N'Gdansk - Oddzial Polnoc'
    )
) AS source (WarehouseID, LocationCode, Description)
    ON target.WarehouseID = source.WarehouseID
    AND target.LocationCode = source.LocationCode
WHEN MATCHED THEN
    UPDATE SET Description = source.Description
WHEN NOT MATCHED THEN
    INSERT (LocationID, WarehouseID, LocationCode, Description)
    VALUES (NEWID(), source.WarehouseID, source.LocationCode, source.Description);

COMMIT TRANSACTION;
GO
