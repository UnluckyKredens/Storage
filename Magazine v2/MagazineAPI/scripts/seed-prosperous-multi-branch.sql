/*
    Dane demonstracyjne dla prosperującej, wielooddziałowej sieci magazynowej.

    Wymagania:
      1. SQL Server.
      2. Baza MagazineAPI utworzona i zaktualizowana migracjami EF Core.

    Skrypt jest transakcyjny i idempotentny:
      - istniejące rekordy demonstracyjne są aktualizowane,
      - brakujące rekordy są dodawane,
      - wielokrotne uruchomienie nie dubluje danych.

    Wszystkie konta demonstracyjne mają hasło: Test123!
    Hash ma stałą sól wyłącznie po to, aby seed był powtarzalny.
    Nie należy używać tego hasła ani sposobu seedowania w środowisku produkcyjnym.
*/

USE [MagazineAPI];
GO

SET QUOTED_IDENTIFIER ON;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    ---------------------------------------------------------------------------
    -- Role
    ---------------------------------------------------------------------------

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

    ---------------------------------------------------------------------------
    -- Uprawnienia i domyślne przypisania ról
    -- Administrator nie potrzebuje wpisów w RolePermissions: API zawsze
    -- przyznaje mu pełny dostęp.
    ---------------------------------------------------------------------------

    MERGE dbo.Permissions AS target
    USING
    (
        VALUES
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000001'), N'products.read', N'Podgląd produktów', N'Wyświetlanie katalogu i szczegółów produktów.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000002'), N'products.manage', N'Zarządzanie produktami', N'Dodawanie, edycja i wycofywanie produktów.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000003'), N'inventory.read', N'Podgląd stanów', N'Wyświetlanie stanów i rezerwacji magazynowych.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000004'), N'inventory.manage', N'Zarządzanie stanami', N'Przyjęcia, wydania, przesunięcia i korekty stanów.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000005'), N'warehouses.read', N'Podgląd magazynów', N'Wyświetlanie magazynów oraz lokalizacji.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000006'), N'warehouses.manage', N'Zarządzanie magazynami', N'Dodawanie i edycja magazynów oraz lokalizacji.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000007'), N'contractors.read', N'Podgląd kontrahentów', N'Wyświetlanie dostawców i odbiorców.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000008'), N'contractors.manage', N'Zarządzanie kontrahentami', N'Dodawanie i edycja kontrahentów.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000009'), N'users.read', N'Podgląd użytkowników', N'Wyświetlanie kont użytkowników.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000010'), N'users.manage', N'Zarządzanie użytkownikami', N'Edycja i usuwanie kont innych niż administratorzy.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000011'), N'dictionaries.manage', N'Zarządzanie słownikami', N'Edycja kategorii i jednostek miary.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000012'), N'stock-documents.manage', N'Tworzenie dokumentów PZ i WZ', N'Tworzenie, edycja i usuwanie szkiców przyjęć oraz wydań.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000013'), N'stock-documents.approve', N'Zatwierdzanie dokumentów PZ i WZ', N'Zatwierdzanie przyjęć i wydań zmieniających stan magazynowy.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000014'), N'stock-documents.read', N'Podgląd dokumentów PZ i WZ', N'Wyświetlanie list dokumentów i ich historii.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000015'), N'stock-documents.receive', N'Odbiór przesyłek', N'Skanowanie kodu, podgląd zawartości i przyjmowanie przesyłek PZ.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000016'), N'stock-shipments.create', N'Tworzenie wysyłek międzyoddziałowych', N'Tworzenie szkiców WZ kierowanych do innego oddziału.')
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

    ---------------------------------------------------------------------------
    -- Jednostki miary
    ---------------------------------------------------------------------------

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

    ---------------------------------------------------------------------------
    -- Kategorie
    ---------------------------------------------------------------------------

    MERGE dbo.Categories AS target
    USING
    (
        VALUES
            (N'Elektronika', N'Komputery, monitory, skanery i urządzenia peryferyjne.'),
            (N'Wyposażenie biura', N'Meble, papier i materiały do codziennej pracy biurowej.'),
            (N'AGD i zaplecze socjalne', N'Wyposażenie kuchni i pomieszczeń pracowniczych.'),
            (N'Narzędzia', N'Elektronarzędzia oraz wyposażenie warsztatowe.'),
            (N'BHP', N'Odzież ochronna i środki bezpieczeństwa pracowników.'),
            (N'Opakowania i logistyka', N'Materiały do pakowania, etykietowania i transportu.'),
            (N'Chemia gospodarcza', N'Środki czystości dla magazynów i biur.'),
            (N'Artykuły sezonowe', N'Produkty o zwiększonej rotacji w wybranych porach roku.')
    ) AS source (Name, Description)
        ON target.Name = source.Name
    WHEN MATCHED THEN
        UPDATE SET Description = source.Description
    WHEN NOT MATCHED THEN
        INSERT (CategoryID, Name, Description)
        VALUES (NEWID(), source.Name, source.Description);

    ---------------------------------------------------------------------------
    -- Kontrahenci
    -- Type odpowiada enumowi ContractorType: Supplier, Customer, Both.
    ---------------------------------------------------------------------------

    MERGE dbo.Contractors AS target
    USING
    (
        VALUES
            (N'TechSource Polska sp. z o.o.', N'5252684101', N'Supplier', N'handel@techsource.pl', N'+48 22 410 20 30', N'ul. Cybernetyki 12, 02-677 Warszawa'),
            (N'OfficePro S.A.', N'7792468102', N'Both', N'bok@officepro.pl', N'+48 61 620 11 22', N'ul. Przemysłowa 18, 60-541 Poznań'),
            (N'SafeWork Polska sp. z o.o.', N'6762519384', N'Supplier', N'zamowienia@safework.pl', N'+48 12 330 44 55', N'ul. Zakopiańska 88, 30-418 Kraków'),
            (N'PackFlow S.A.', N'5833391205', N'Supplier', N'sprzedaz@packflow.pl', N'+48 58 700 32 10', N'ul. Kontenerowa 7, 80-601 Gdańsk'),
            (N'CleanPoint sp. z o.o.', N'6342918750', N'Supplier', N'biuro@cleanpoint.pl', N'+48 32 440 15 80', N'ul. Chemiczna 5, 40-246 Katowice'),
            (N'North Retail Group S.A.', N'5842789012', N'Customer', N'zakupy@northretail.pl', N'+48 58 520 70 80', N'al. Grunwaldzka 415, 80-309 Gdańsk'),
            (N'Market24 sp. z o.o.', N'5272931846', N'Customer', N'logistyka@market24.pl', N'+48 22 240 24 24', N'ul. Annopol 17, 03-236 Warszawa'),
            (N'BuildMaster S.A.', N'9452247619', N'Customer', N'dostawy@buildmaster.pl', N'+48 12 610 90 20', N'ul. Nowohucka 44, 31-580 Kraków'),
            (N'WorkSpace Solutions sp. z o.o.', N'7831814407', N'Both', N'operacje@workspace.pl', N'+48 61 850 33 00', N'ul. Bukowska 148, 60-198 Poznań'),
            (N'Baltic Commerce sp. z o.o.', N'9571120348', N'Customer', N'magazyn@balticcommerce.pl', N'+48 58 301 12 90', N'ul. Hutnicza 16, 81-061 Gdynia')
    ) AS source (Name, TaxNumber, Type, Email, Phone, Address)
        ON target.TaxNumber = source.TaxNumber
    WHEN MATCHED THEN
        UPDATE SET
            Name = source.Name,
            Type = source.Type,
            Email = source.Email,
            Phone = source.Phone,
            Address = source.Address
    WHEN NOT MATCHED THEN
        INSERT (ContractorID, Name, TaxNumber, Type, Email, Phone, Address)
        VALUES (NEWID(), source.Name, source.TaxNumber, source.Type, source.Email, source.Phone, source.Address);

    ---------------------------------------------------------------------------
    -- Oddziały magazynowe
    ---------------------------------------------------------------------------

    MERGE dbo.Warehouses AS target
    USING
    (
        VALUES
            (N'Warszawa - Centrum Dystrybucyjne', N'ul. Logistyczna 1, 05-090 Sękocin Stary', N'Główne centrum dystrybucyjne, cross-docking i obsługa e-commerce.'),
            (N'Poznań - Oddział Zachód', N'ul. Magazynowa 24, 62-080 Tarnowo Podgórne', N'Obsługa zachodniej Polski i dostaw transgranicznych.'),
            (N'Kraków - Oddział Południe', N'ul. Przemysłowa 42, 32-085 Modlniczka', N'Obsługa Małopolski, Śląska i regionów południowych.'),
            (N'Gdańsk - Oddział Północ', N'ul. Kontenerowa 19, 80-601 Gdańsk', N'Obsługa północnej Polski oraz dostaw portowych.')
    ) AS source (Name, Address, Description)
        ON target.Name = source.Name
    WHEN MATCHED THEN
        UPDATE SET
            Address = source.Address,
            Description = source.Description
    WHEN NOT MATCHED THEN
        INSERT (WarehouseID, Name, Address, Description)
        VALUES (NEWID(), source.Name, source.Address, source.Description);

    ---------------------------------------------------------------------------
    -- Lokalizacje w każdym oddziale
    ---------------------------------------------------------------------------

    MERGE dbo.Locations AS target
    USING
    (
        SELECT
            warehouse.WarehouseID,
            seed.LocationCode,
            seed.Description
        FROM
        (
            VALUES
                (N'Warszawa - Centrum Dystrybucyjne', N'REC-01', N'Strefa przyjęć i kontroli dostaw'),
                (N'Warszawa - Centrum Dystrybucyjne', N'A-01', N'Regały wysokiego składowania - sektor A'),
                (N'Warszawa - Centrum Dystrybucyjne', N'B-01', N'Regały wysokiego składowania - sektor B'),
                (N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', N'Strefa szybkiej kompletacji zamówień'),
                (N'Warszawa - Centrum Dystrybucyjne', N'RET-01', N'Zwroty i kontrola jakości'),

                (N'Poznań - Oddział Zachód', N'REC-01', N'Strefa przyjęć i kontroli dostaw'),
                (N'Poznań - Oddział Zachód', N'A-01', N'Regały główne - sektor A'),
                (N'Poznań - Oddział Zachód', N'B-01', N'Regały główne - sektor B'),
                (N'Poznań - Oddział Zachód', N'PICK-01', N'Strefa szybkiej kompletacji zamówień'),
                (N'Poznań - Oddział Zachód', N'RET-01', N'Zwroty i reklamacje'),

                (N'Kraków - Oddział Południe', N'REC-01', N'Strefa przyjęć i kontroli dostaw'),
                (N'Kraków - Oddział Południe', N'A-01', N'Regały główne - sektor A'),
                (N'Kraków - Oddział Południe', N'B-01', N'Regały główne - sektor B'),
                (N'Kraków - Oddział Południe', N'PICK-01', N'Strefa szybkiej kompletacji zamówień'),
                (N'Kraków - Oddział Południe', N'RET-01', N'Zwroty i reklamacje'),

                (N'Gdańsk - Oddział Północ', N'REC-01', N'Strefa przyjęć oraz odpraw dostaw portowych'),
                (N'Gdańsk - Oddział Północ', N'A-01', N'Regały główne - sektor A'),
                (N'Gdańsk - Oddział Północ', N'B-01', N'Regały główne - sektor B'),
                (N'Gdańsk - Oddział Północ', N'PICK-01', N'Strefa szybkiej kompletacji zamówień'),
                (N'Gdańsk - Oddział Północ', N'RET-01', N'Zwroty i reklamacje')
        ) AS seed (WarehouseName, LocationCode, Description)
        INNER JOIN dbo.Warehouses AS warehouse
            ON warehouse.Name = seed.WarehouseName
    ) AS source
        ON target.WarehouseID = source.WarehouseID
        AND target.LocationCode = source.LocationCode
    WHEN MATCHED THEN
        UPDATE SET Description = source.Description
    WHEN NOT MATCHED THEN
        INSERT (LocationID, WarehouseID, LocationCode, Description)
        VALUES (NEWID(), source.WarehouseID, source.LocationCode, source.Description);

    ---------------------------------------------------------------------------
    -- Produkty
    ---------------------------------------------------------------------------

    MERGE dbo.Products AS target
    USING
    (
        SELECT
            seed.Name,
            seed.SKU,
            seed.Barcode,
            seed.Description,
            unitOfMeasure.UnitOfMeasureID,
            category.CategoryID,
            seed.PurchasePrice,
            seed.SalePrice,
            seed.IsActive
        FROM
        (
            VALUES
                (N'Laptop biznesowy ProBook 14', N'EL-LAP-001', N'5901000000011', N'Laptop 14 cali, 16 GB RAM, SSD 512 GB.', N'szt.', N'Elektronika', CAST(2890.00 AS decimal(18,2)), CAST(3699.00 AS decimal(18,2)), CAST(1 AS bit)),
                (N'Monitor LED 27 cali QHD', N'EL-MON-027', N'5901000000028', N'Monitor biurowy QHD z regulacją wysokości.', N'szt.', N'Elektronika', 820.00, 1099.00, 1),
                (N'Skaner kodów kreskowych 2D', N'EL-SCN-002', N'5901000000035', N'Bezprzewodowy skaner magazynowy ze stacją dokującą.', N'szt.', N'Elektronika', 410.00, 599.00, 1),
                (N'Drukarka etykiet termicznych', N'EL-PRN-004', N'5901000000042', N'Drukarka etykiet 100 x 150 mm do stanowisk pakowania.', N'szt.', N'Elektronika', 690.00, 949.00, 1),
                (N'Krzesło ergonomiczne ErgoFlex', N'BI-KRZ-010', N'5901000000059', N'Krzesło z regulacją podparcia lędźwiowego.', N'szt.', N'Wyposażenie biura', 540.00, 799.00, 1),
                (N'Papier ksero A4 80 g', N'BI-PAP-A4', N'5901000000066', N'Opakowanie 5 ryz po 500 arkuszy.', N'op.', N'Wyposażenie biura', 82.00, 119.00, 1),
                (N'Ekspres automatyczny OfficeBar', N'AG-EKS-015', N'5901000000073', N'Automatyczny ekspres do kawy do zaplecza pracowniczego.', N'szt.', N'AGD i zaplecze socjalne', 1280.00, 1699.00, 1),
                (N'Czajnik elektryczny 1,7 l', N'AG-CZA-017', N'5901000000080', N'Czajnik ze stali nierdzewnej z automatycznym wyłącznikiem.', N'szt.', N'AGD i zaplecze socjalne', 92.00, 149.00, 1),
                (N'Wiertarko-wkrętarka 18 V', N'NA-WWK-018', N'5901000000097', N'Zestaw z dwoma akumulatorami i walizką.', N'szt.', N'Narzędzia', 430.00, 649.00, 1),
                (N'Zestaw narzędzi serwisowych 108 el.', N'NA-ZES-108', N'5901000000103', N'Komplet kluczy i nasadek dla serwisu magazynowego.', N'szt.', N'Narzędzia', 310.00, 469.00, 1),
                (N'Rękawice ochronne powlekane', N'BH-REK-012', N'5901000000110', N'Opakowanie 12 par rękawic roboczych.', N'op.', N'BHP', 42.00, 69.00, 1),
                (N'Kamizelka ostrzegawcza', N'BH-KAM-001', N'5901000000127', N'Kamizelka odblaskowa klasy 2.', N'szt.', N'BHP', 18.00, 34.90, 1),
                (N'Folia stretch transparentna', N'OP-FOL-500', N'5901000000134', N'Folia maszynowa o szerokości 500 mm.', N'kg', N'Opakowania i logistyka', 8.40, 12.90, 1),
                (N'Etykiety termiczne 100 x 150 mm', N'OP-ETY-150', N'5901000000141', N'Opakowanie 6 rolek etykiet logistycznych.', N'op.', N'Opakowania i logistyka', 96.00, 139.00, 1),
                (N'Paleta EUR EPAL', N'OP-PAL-EUR', N'5901000000158', N'Drewniana paleta wielokrotnego użytku 1200 x 800 mm.', N'szt.', N'Opakowania i logistyka', 48.00, 69.00, 1),
                (N'Płyn do mycia powierzchni przemysłowych', N'CH-PLY-005', N'5901000000165', N'Koncentrat do posadzek i powierzchni magazynowych.', N'l', N'Chemia gospodarcza', 9.80, 16.50, 1),
                (N'Sól drogowa workowana', N'SE-SOL-025', N'5901000000172', N'Worek 25 kg soli do utrzymania placów i ramp.', N'kg', N'Artykuły sezonowe', 0.72, 1.15, 1),
                (N'Wentylator przemysłowy 120 W', N'SE-WEN-120', N'5901000000189', N'Wentylator podłogowy do hal i stref kompletacji.', N'szt.', N'Artykuły sezonowe', 245.00, 359.00, 1)
        ) AS seed (Name, SKU, Barcode, Description, UnitSymbol, CategoryName, PurchasePrice, SalePrice, IsActive)
        INNER JOIN dbo.UnitsOfMeasure AS unitOfMeasure
            ON unitOfMeasure.Symbol = seed.UnitSymbol
        INNER JOIN dbo.Categories AS category
            ON category.Name = seed.CategoryName
    ) AS source
        ON target.SKU = source.SKU
    WHEN MATCHED THEN
        UPDATE SET
            Name = source.Name,
            Barcode = source.Barcode,
            Description = source.Description,
            UnitOfMeasureID = source.UnitOfMeasureID,
            CategoryID = source.CategoryID,
            PurchasePrice = source.PurchasePrice,
            SalePrice = source.SalePrice,
            IsActive = source.IsActive
    WHEN NOT MATCHED THEN
        INSERT
        (
            ProductID,
            Name,
            SKU,
            Barcode,
            Description,
            UnitOfMeasureID,
            CategoryID,
            PurchasePrice,
            SalePrice,
            IsActive
        )
        VALUES
        (
            NEWID(),
            source.Name,
            source.SKU,
            source.Barcode,
            source.Description,
            source.UnitOfMeasureID,
            source.CategoryID,
            source.PurchasePrice,
            source.SalePrice,
            source.IsActive
        );

    ---------------------------------------------------------------------------
    -- Użytkownicy
    ---------------------------------------------------------------------------

    DECLARE @DemoPasswordHash nvarchar(512) =
        N'100000.TWFnYXppbmVEZW1vMjAyNiE=.u5PT/rQoclLtH/vS5ZM0TVi1vQNpExjQjlh7mRxB6sQ=';

    MERGE dbo.Users AS target
    USING
    (
        SELECT seed.UserID, seed.Login, seed.FirstName, seed.LastName, seed.Email,
            seed.RoleID, warehouse.WarehouseID
        FROM (VALUES
            (CONVERT(uniqueidentifier, 'a0000000-0000-0000-0000-000000000001'), N'admin', N'Anna', N'Nowak', N'admin@magazine.local', CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000001'), CAST(NULL AS nvarchar(200))),
            (CONVERT(uniqueidentifier, 'a0000000-0000-0000-0000-000000000002'), N'kierownik.waw', N'Marek', N'Kowalski', N'marek.kowalski@magazine.local', CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), N'Warszawa - Centrum Dystrybucyjne'),
            (CONVERT(uniqueidentifier, 'a0000000-0000-0000-0000-000000000003'), N'kierownik.poz', N'Joanna', N'Wiśniewska', N'joanna.wisniewska@magazine.local', CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), N'Poznań - Oddział Zachód'),
            (CONVERT(uniqueidentifier, 'a0000000-0000-0000-0000-000000000004'), N'kierownik.krk', N'Piotr', N'Zieliński', N'piotr.zielinski@magazine.local', CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), N'Kraków - Oddział Południe'),
            (CONVERT(uniqueidentifier, 'a0000000-0000-0000-0000-000000000005'), N'kierownik.gdn', N'Katarzyna', N'Lewandowska', N'katarzyna.lewandowska@magazine.local', CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), N'Gdańsk - Oddział Północ'),
            (CONVERT(uniqueidentifier, 'a0000000-0000-0000-0000-000000000006'), N'magazynier.01', N'Tomasz', N'Wójcik', N'tomasz.wojcik@magazine.local', CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), N'Warszawa - Centrum Dystrybucyjne'),
            (CONVERT(uniqueidentifier, 'a0000000-0000-0000-0000-000000000007'), N'operator.02', N'Agnieszka', N'Kamińska', N'agnieszka.kaminska@magazine.local', CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), N'Poznań - Oddział Zachód'),
            (CONVERT(uniqueidentifier, 'a0000000-0000-0000-0000-000000000008'), N'kontrola.03', N'Paweł', N'Kaczmarek', N'pawel.kaczmarek@magazine.local', CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), N'Kraków - Oddział Południe')
        ) AS seed (UserID, Login, FirstName, LastName, Email, RoleID, WarehouseName)
        LEFT JOIN dbo.Warehouses AS warehouse ON warehouse.Name = seed.WarehouseName
    ) AS source
        ON target.Login = source.Login
    WHEN MATCHED THEN
        UPDATE SET
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email,
            PasswordHash = @DemoPasswordHash,
            RoleID = source.RoleID,
            WarehouseID = source.WarehouseID
    WHEN NOT MATCHED THEN
        INSERT (UserID, Login, FirstName, LastName, Email, PasswordHash, RoleID, WarehouseID)
        VALUES
        (
            source.UserID,
            source.Login,
            source.FirstName,
            source.LastName,
            source.Email,
            @DemoPasswordHash,
            source.RoleID,
            source.WarehouseID
        );

    ---------------------------------------------------------------------------
    -- Stany magazynowe
    -- AvailableQuantity jest kolumną wyliczaną, dlatego nie jest wstawiana.
    ---------------------------------------------------------------------------

    MERGE dbo.Inventory AS target
    USING
    (
        SELECT
            product.ProductID,
            location.LocationID,
            seed.Quantity,
            seed.ReservedQuantity
        FROM
        (
            VALUES
                (N'EL-LAP-001', N'Warszawa - Centrum Dystrybucyjne', N'A-01', CAST(86.000 AS decimal(18,3)), CAST(18.000 AS decimal(18,3))),
                (N'EL-LAP-001', N'Poznań - Oddział Zachód', N'PICK-01', 28.000, 7.000),
                (N'EL-LAP-001', N'Kraków - Oddział Południe', N'PICK-01', 24.000, 5.000),
                (N'EL-LAP-001', N'Gdańsk - Oddział Północ', N'PICK-01', 21.000, 3.000),

                (N'EL-MON-027', N'Warszawa - Centrum Dystrybucyjne', N'B-01', 160.000, 32.000),
                (N'EL-MON-027', N'Poznań - Oddział Zachód', N'A-01', 52.000, 9.000),
                (N'EL-MON-027', N'Kraków - Oddział Południe', N'A-01', 47.000, 11.000),
                (N'EL-MON-027', N'Gdańsk - Oddział Północ', N'A-01', 38.000, 6.000),

                (N'EL-SCN-002', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 74.000, 16.000),
                (N'EL-SCN-002', N'Poznań - Oddział Zachód', N'PICK-01', 29.000, 4.000),
                (N'EL-SCN-002', N'Gdańsk - Oddział Północ', N'PICK-01', 26.000, 5.000),
                (N'EL-PRN-004', N'Warszawa - Centrum Dystrybucyjne', N'A-01', 63.000, 14.000),
                (N'EL-PRN-004', N'Kraków - Oddział Południe', N'PICK-01', 22.000, 4.000),

                (N'BI-KRZ-010', N'Warszawa - Centrum Dystrybucyjne', N'B-01', 95.000, 20.000),
                (N'BI-KRZ-010', N'Poznań - Oddział Zachód', N'B-01', 44.000, 8.000),
                (N'BI-KRZ-010', N'Kraków - Oddział Południe', N'B-01', 39.000, 7.000),
                (N'BI-PAP-A4', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 540.000, 84.000),
                (N'BI-PAP-A4', N'Poznań - Oddział Zachód', N'PICK-01', 210.000, 35.000),
                (N'BI-PAP-A4', N'Kraków - Oddział Południe', N'PICK-01', 185.000, 28.000),
                (N'BI-PAP-A4', N'Gdańsk - Oddział Północ', N'PICK-01', 175.000, 21.000),

                (N'AG-EKS-015', N'Warszawa - Centrum Dystrybucyjne', N'A-01', 38.000, 6.000),
                (N'AG-EKS-015', N'Poznań - Oddział Zachód', N'A-01', 17.000, 3.000),
                (N'AG-CZA-017', N'Kraków - Oddział Południe', N'PICK-01', 68.000, 12.000),
                (N'AG-CZA-017', N'Gdańsk - Oddział Północ', N'PICK-01', 55.000, 9.000),

                (N'NA-WWK-018', N'Warszawa - Centrum Dystrybucyjne', N'B-01', 92.000, 19.000),
                (N'NA-WWK-018', N'Kraków - Oddział Południe', N'B-01', 41.000, 7.000),
                (N'NA-ZES-108', N'Poznań - Oddział Zachód', N'B-01', 56.000, 11.000),
                (N'NA-ZES-108', N'Gdańsk - Oddział Północ', N'B-01', 35.000, 5.000),

                (N'BH-REK-012', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 410.000, 65.000),
                (N'BH-REK-012', N'Poznań - Oddział Zachód', N'PICK-01', 180.000, 28.000),
                (N'BH-REK-012', N'Kraków - Oddział Południe', N'PICK-01', 165.000, 32.000),
                (N'BH-KAM-001', N'Gdańsk - Oddział Północ', N'PICK-01', 230.000, 40.000),

                (N'OP-FOL-500', N'Warszawa - Centrum Dystrybucyjne', N'A-01', 1250.000, 180.000),
                (N'OP-FOL-500', N'Poznań - Oddział Zachód', N'A-01', 620.000, 90.000),
                (N'OP-FOL-500', N'Kraków - Oddział Południe', N'A-01', 580.000, 72.000),
                (N'OP-FOL-500', N'Gdańsk - Oddział Północ', N'A-01', 490.000, 64.000),
                (N'OP-ETY-150', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 360.000, 54.000),
                (N'OP-ETY-150', N'Poznań - Oddział Zachód', N'PICK-01', 145.000, 24.000),
                (N'OP-PAL-EUR', N'Warszawa - Centrum Dystrybucyjne', N'B-01', 680.000, 95.000),
                (N'OP-PAL-EUR', N'Gdańsk - Oddział Północ', N'B-01', 310.000, 42.000),

                (N'CH-PLY-005', N'Warszawa - Centrum Dystrybucyjne', N'A-01', 720.000, 86.000),
                (N'CH-PLY-005', N'Kraków - Oddział Południe', N'A-01', 290.000, 37.000),
                (N'SE-SOL-025', N'Poznań - Oddział Zachód', N'B-01', 4200.000, 550.000),
                (N'SE-SOL-025', N'Gdańsk - Oddział Północ', N'B-01', 5100.000, 720.000),
                (N'SE-WEN-120', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 105.000, 24.000),
                (N'SE-WEN-120', N'Kraków - Oddział Południe', N'PICK-01', 62.000, 13.000)
        ) AS seed (SKU, WarehouseName, LocationCode, Quantity, ReservedQuantity)
        INNER JOIN dbo.Products AS product
            ON product.SKU = seed.SKU
        INNER JOIN dbo.Warehouses AS warehouse
            ON warehouse.Name = seed.WarehouseName
        INNER JOIN dbo.Locations AS location
            ON location.WarehouseID = warehouse.WarehouseID
            AND location.LocationCode = seed.LocationCode
    ) AS source
        ON target.ProductID = source.ProductID
        AND target.LocationID = source.LocationID
    WHEN MATCHED THEN
        UPDATE SET
            Quantity = source.Quantity,
            ReservedQuantity = source.ReservedQuantity
    WHEN NOT MATCHED THEN
        INSERT (InventoryID, ProductID, LocationID, Quantity, ReservedQuantity)
        VALUES (NEWID(), source.ProductID, source.LocationID, source.Quantity, source.ReservedQuantity);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

-------------------------------------------------------------------------------
-- Kontrola wyniku
-------------------------------------------------------------------------------

SELECT N'Roles' AS TableName, COUNT(*) AS RecordCount FROM dbo.Roles
UNION ALL SELECT N'Permissions', COUNT(*) FROM dbo.Permissions
UNION ALL SELECT N'RolePermissions', COUNT(*) FROM dbo.RolePermissions
UNION ALL SELECT N'Users', COUNT(*) FROM dbo.Users
UNION ALL SELECT N'Categories', COUNT(*) FROM dbo.Categories
UNION ALL SELECT N'UnitsOfMeasure', COUNT(*) FROM dbo.UnitsOfMeasure
UNION ALL SELECT N'Contractors', COUNT(*) FROM dbo.Contractors
UNION ALL SELECT N'Warehouses', COUNT(*) FROM dbo.Warehouses
UNION ALL SELECT N'Locations', COUNT(*) FROM dbo.Locations
UNION ALL SELECT N'Products', COUNT(*) FROM dbo.Products
UNION ALL SELECT N'Inventory', COUNT(*) FROM dbo.Inventory;

SELECT
    warehouse.Name AS Warehouse,
    COUNT(DISTINCT inventory.ProductID) AS ProductCount,
    SUM(inventory.Quantity) AS TotalQuantity,
    SUM(inventory.ReservedQuantity) AS ReservedQuantity,
    SUM(inventory.AvailableQuantity) AS AvailableQuantity,
    CAST(SUM(inventory.AvailableQuantity * product.SalePrice) AS decimal(18, 2)) AS AvailableStockSaleValue
FROM dbo.Warehouses AS warehouse
INNER JOIN dbo.Locations AS location
    ON location.WarehouseID = warehouse.WarehouseID
INNER JOIN dbo.Inventory AS inventory
    ON inventory.LocationID = location.LocationID
INNER JOIN dbo.Products AS product
    ON product.ProductID = inventory.ProductID
GROUP BY warehouse.Name
ORDER BY warehouse.Name;
GO
