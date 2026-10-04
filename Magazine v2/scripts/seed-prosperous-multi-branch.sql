/*
    Dane demonstracyjne dla prosperującej, wielooddziałowej hurtowni komputerowej.

    Wymagania:
      1. SQL Server.
      2. Bazy MagazineAPIDev oraz MagazineHistory utworzone i zaktualizowane migracjami EF Core.

    Skrypt jest transakcyjny i idempotentny:
      - istniejące rekordy demonstracyjne są aktualizowane,
      - brakujące rekordy są dodawane,
      - wielokrotne uruchomienie nie dubluje danych.

    Wszystkie konta demonstracyjne mają hasło: Test123!
    Hash ma stałą sól wyłącznie po to, aby seed był powtarzalny.
    Nie należy używać tego hasła ani sposobu seedowania w środowisku produkcyjnym.
*/

USE [MagazineAPIDev];
GO

SET QUOTED_IDENTIFIER ON;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION

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
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000012'), N'shipments.read', N'Podgląd wysyłek', N'Wyświetlanie wysyłek między magazynami.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000013'), N'shipments.create', N'Tworzenie wysyłek', N'Tworzenie wysyłek z magazynu pracownika.'),
            (CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000014'), N'shipments.approve', N'Akceptacja wysyłek', N'Akceptowanie wysyłek i przygotowanie WZ.')
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
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000001')),
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000002')),
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000003')),
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000004')),
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000005')),
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000007')),
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000012')),
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000013')),
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000002'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000014')),
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000001')),
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000003')),
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000012')),
            (CONVERT(uniqueidentifier, '10000000-0000-0000-0000-000000000003'), CONVERT(uniqueidentifier, '20000000-0000-0000-0000-000000000013'))
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
            (N'Komplet', N'kpl.'),
            (N'Metr', N'm')
    ) AS source (Name, Symbol)
        ON target.Symbol = source.Symbol
    WHEN MATCHED THEN
        UPDATE SET Name = source.Name
    WHEN NOT MATCHED THEN
        INSERT (UnitOfMeasureID, Name, Symbol)
        VALUES (NEWID(), source.Name, source.Symbol);

    ---------------------------------------------------------------------------
    -- Kategorie sprzętowe
    ---------------------------------------------------------------------------

    MERGE dbo.Categories AS target
    USING
    (
        VALUES
            (N'Laptopy biznesowe', N'Notebooki klasy biznesowej, stacje robocze mobilne i ultrabooki.'),
            (N'Komputery stacjonarne', N'Zestawy desktop, mini PC oraz stacje robocze.'),
            (N'Monitory', N'Monitory biurowe, graficzne, gamingowe i wielkoformatowe.'),
            (N'Podzespoły PC', N'Procesory, płyty główne, zasilacze, obudowy i chłodzenie.'),
            (N'Pamięci i dyski', N'RAM, SSD, HDD, karty pamięci i nośniki zewnętrzne.'),
            (N'Karty graficzne', N'GPU do stacji roboczych, gamingu i akceleracji obliczeń.'),
            (N'Sieć i serwery', N'Serwery, switche, routery, firewalle, NAS i osprzęt rack.'),
            (N'Peryferia', N'Klawiatury, myszy, zestawy konferencyjne, skanery i czytniki.'),
            (N'Drukarki i skanery', N'Urządzenia drukujące, skanujące oraz materiały eksploatacyjne.'),
            (N'Akcesoria komputerowe', N'Kable, adaptery, stacje dokujące, uchwyty i drobne akcesoria.')
    ) AS source (Name, Description)
        ON target.Name = source.Name
    WHEN MATCHED THEN
        UPDATE SET Description = source.Description
    WHEN NOT MATCHED THEN
        INSERT (CategoryID, Name, Description)
        VALUES (NEWID(), source.Name, source.Description);

    ---------------------------------------------------------------------------
    -- Kontrahenci IT
    ---------------------------------------------------------------------------

    MERGE dbo.Contractors AS target
    USING
    (
        VALUES
            (N'CoreDistrib Polska sp. z o.o.', N'5252684101', N'Supplier', N'handel@coredistrib.pl', N'+48 22 410 20 30', N'ul. Cybernetyki 12, 02-677 Warszawa'),
            (N'ByteMarket Dystrybucja S.A.', N'7792468102', N'Both', N'b2b@bytemarket.pl', N'+48 61 620 11 22', N'ul. Przemysłowa 18, 60-541 Poznań'),
            (N'RackPoint Systems sp. z o.o.', N'6762519384', N'Supplier', N'zamowienia@rackpoint.pl', N'+48 12 330 44 55', N'ul. Zakopiańska 88, 30-418 Kraków'),
            (N'NetWave Hardware S.A.', N'5833391205', N'Supplier', N'sprzedaz@netwave.pl', N'+48 58 700 32 10', N'ul. Kontenerowa 7, 80-601 Gdańsk'),
            (N'PrintLab Business sp. z o.o.', N'6342918750', N'Supplier', N'biuro@printlab.pl', N'+48 32 440 15 80', N'ul. Technologiczna 5, 40-246 Katowice'),
            (N'NorthTech Retail Group S.A.', N'5842789012', N'Customer', N'zakupy@northtech.pl', N'+48 58 520 70 80', N'al. Grunwaldzka 415, 80-309 Gdańsk'),
            (N'DeskFleet Pro sp. z o.o.', N'5272931846', N'Customer', N'logistyka@deskfleet.pl', N'+48 22 240 24 24', N'ul. Annopol 17, 03-236 Warszawa'),
            (N'Compute4Business S.A.', N'9452247619', N'Customer', N'dostawy@compute4business.pl', N'+48 12 610 90 20', N'ul. Nowohucka 44, 31-580 Kraków'),
            (N'Workstation House sp. z o.o.', N'7831814407', N'Both', N'operacje@workstationhouse.pl', N'+48 61 850 33 00', N'ul. Bukowska 148, 60-198 Poznań'),
            (N'Baltic IT Commerce sp. z o.o.', N'9571120348', N'Customer', N'magazyn@balticit.pl', N'+48 58 301 12 90', N'ul. Hutnicza 16, 81-061 Gdynia')
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
            (N'Warszawa - Centrum Dystrybucyjne', N'ul. Logistyczna 1, 05-090 Sękocin Stary', N'Główne centrum dystrybucyjne sprzętu komputerowego, cross-docking i obsługa B2B.'),
            (N'Poznań - Oddział Zachód', N'ul. Magazynowa 24, 62-080 Tarnowo Podgórne', N'Obsługa resellerów z zachodniej Polski i dostaw transgranicznych.'),
            (N'Kraków - Oddział Południe', N'ul. Przemysłowa 42, 32-085 Modlniczka', N'Obsługa Małopolski, Śląska i wdrożeń sprzętowych na południu.'),
            (N'Gdańsk - Oddział Północ', N'ul. Kontenerowa 19, 80-601 Gdańsk', N'Obsługa północnej Polski oraz dostaw portowych sprzętu IT.')
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
                (N'Warszawa - Centrum Dystrybucyjne', N'REC-01', N'Strefa przyjęć, serializacji i kontroli dostaw'),
                (N'Warszawa - Centrum Dystrybucyjne', N'A-01', N'Regały wysokiego składowania - komputery i monitory'),
                (N'Warszawa - Centrum Dystrybucyjne', N'B-01', N'Regały wysokiego składowania - podzespoły i sieć'),
                (N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', N'Strefa szybkiej kompletacji sprzętu'),
                (N'Warszawa - Centrum Dystrybucyjne', N'RET-01', N'Zwroty, DOA i kontrola gwarancyjna'),
                (N'Poznań - Oddział Zachód', N'REC-01', N'Strefa przyjęć, serializacji i kontroli dostaw'),
                (N'Poznań - Oddział Zachód', N'A-01', N'Regały główne - komputery i monitory'),
                (N'Poznań - Oddział Zachód', N'B-01', N'Regały główne - podzespoły i sieć'),
                (N'Poznań - Oddział Zachód', N'PICK-01', N'Strefa szybkiej kompletacji sprzętu'),
                (N'Poznań - Oddział Zachód', N'RET-01', N'Zwroty i reklamacje sprzętu'),
                (N'Kraków - Oddział Południe', N'REC-01', N'Strefa przyjęć, serializacji i kontroli dostaw'),
                (N'Kraków - Oddział Południe', N'A-01', N'Regały główne - komputery i monitory'),
                (N'Kraków - Oddział Południe', N'B-01', N'Regały główne - podzespoły i sieć'),
                (N'Kraków - Oddział Południe', N'PICK-01', N'Strefa szybkiej kompletacji sprzętu'),
                (N'Kraków - Oddział Południe', N'RET-01', N'Zwroty i reklamacje sprzętu'),
                (N'Gdańsk - Oddział Północ', N'REC-01', N'Strefa przyjęć oraz odpraw dostaw portowych'),
                (N'Gdańsk - Oddział Północ', N'A-01', N'Regały główne - komputery i monitory'),
                (N'Gdańsk - Oddział Północ', N'B-01', N'Regały główne - podzespoły i sieć'),
                (N'Gdańsk - Oddział Północ', N'PICK-01', N'Strefa szybkiej kompletacji sprzętu'),
                (N'Gdańsk - Oddział Północ', N'RET-01', N'Zwroty i reklamacje sprzętu')
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
    -- Produkty sprzętowe
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
                (N'Laptop biznesowy 14" i5/16GB/512SSD', N'NB-BIZ-14512', N'5902000000015', N'Notebook 14 cali, Intel Core i5, 16 GB RAM, SSD 512 GB, Windows Pro.', N'szt.', N'Laptopy biznesowe', CAST(2890.00 AS decimal(18,2)), CAST(3699.00 AS decimal(18,2)), CAST(1 AS bit)),
                (N'Laptop biznesowy 15" Ryzen 7/32GB/1TB', N'NB-BIZ-151TB', N'5902000000022', N'Notebook 15,6 cala, Ryzen 7, 32 GB RAM, SSD 1 TB, Windows Pro.', N'szt.', N'Laptopy biznesowe', 3820.00, 4899.00, 1),
                (N'Stacja robocza i7/64GB/2TB/RTX', N'PC-WS-I7RTX', N'5902000000039', N'Komputer dla grafiki i CAD z kartą RTX, 64 GB RAM i SSD 2 TB.', N'szt.', N'Komputery stacjonarne', 6120.00, 7899.00, 1),
                (N'Mini PC i5/16GB/512SSD', N'PC-MINI-I5', N'5902000000046', N'Kompaktowy komputer biurowy VESA, Intel Core i5, 16 GB RAM.', N'szt.', N'Komputery stacjonarne', 1740.00, 2299.00, 1),
                (N'Monitor 27" QHD IPS USB-C', N'MON-27-QHD-USBC', N'5902000000053', N'Monitor 27 cali QHD z USB-C, regulacją wysokości i pivot.', N'szt.', N'Monitory', 820.00, 1099.00, 1),
                (N'Monitor 34" Ultrawide WQHD', N'MON-34-UWQHD', N'5902000000060', N'Monitor panoramiczny 34 cale do stanowisk analitycznych i graficznych.', N'szt.', N'Monitory', 1510.00, 1999.00, 1),
                (N'Procesor 8C/16T 4.9GHz BOX', N'CPU-8C-49BOX', N'5902000000077', N'Procesor desktopowy 8 rdzeni, 16 wątków, wersja BOX.', N'szt.', N'Podzespoły PC', 870.00, 1199.00, 1),
                (N'Płyta główna B760 ATX DDR5', N'MB-B760-ATX-D5', N'5902000000084', N'Płyta ATX z DDR5, M.2 PCIe 4.0 i 2.5GbE.', N'szt.', N'Podzespoły PC', 520.00, 749.00, 1),
                (N'Zasilacz ATX 750W 80+ Gold', N'PSU-750-GOLD', N'5902000000091', N'Modularny zasilacz ATX 750 W z certyfikatem 80+ Gold.', N'szt.', N'Podzespoły PC', 340.00, 499.00, 1),
                (N'Pamięć RAM DDR5 32GB 5600MHz', N'RAM-DDR5-32-5600', N'5902000000107', N'Zestaw 2x16 GB DDR5 5600 MHz CL36.', N'kpl.', N'Pamięci i dyski', 350.00, 529.00, 1),
                (N'Dysk SSD NVMe 1TB PCIe 4.0', N'SSD-NVME-1TB-G4', N'5902000000114', N'Dysk M.2 NVMe 1 TB, PCIe 4.0, radiator w zestawie.', N'szt.', N'Pamięci i dyski', 265.00, 399.00, 1),
                (N'Dysk SSD NVMe 2TB PCIe 4.0', N'SSD-NVME-2TB-G4', N'5902000000121', N'Dysk M.2 NVMe 2 TB do stacji roboczych i laptopów.', N'szt.', N'Pamięci i dyski', 470.00, 699.00, 1),
                (N'Karta graficzna RTX 4070 12GB', N'GPU-RTX4070-12', N'5902000000138', N'Karta graficzna 12 GB GDDR6X do stacji roboczych i gamingu.', N'szt.', N'Karty graficzne', 2360.00, 2999.00, 1),
                (N'Karta graficzna RTX 4060 8GB', N'GPU-RTX4060-8', N'5902000000145', N'Karta graficzna 8 GB do zestawów desktop i stanowisk graficznych.', N'szt.', N'Karty graficzne', 1260.00, 1699.00, 1),
                (N'Serwer rack 1U Xeon/32GB/2x960SSD', N'SRV-1U-XEON', N'5902000000152', N'Serwer rack 1U z kontrolerem RAID, 32 GB ECC i dwoma SSD.', N'szt.', N'Sieć i serwery', 7850.00, 9999.00, 1),
                (N'Switch zarządzalny 24p PoE+', N'NET-SW24-POE', N'5902000000169', N'Switch 24 porty Gigabit PoE+, uplinki SFP i zarządzanie L2.', N'szt.', N'Sieć i serwery', 1180.00, 1599.00, 1),
                (N'Router VPN dual WAN', N'NET-RTR-VPN-DW', N'5902000000176', N'Router biznesowy z VPN, dual WAN i filtrowaniem ruchu.', N'szt.', N'Sieć i serwery', 690.00, 949.00, 1),
                (N'Klawiatura mechaniczna biznesowa', N'PER-KBD-MECH', N'5902000000183', N'Klawiatura mechaniczna low-profile z układem US/PL.', N'szt.', N'Peryferia', 230.00, 349.00, 1),
                (N'Mysz bezprzewodowa ergonomiczna', N'PER-MSE-ERG', N'5902000000190', N'Mysz bezprzewodowa z odbiornikiem USB i Bluetooth.', N'szt.', N'Peryferia', 115.00, 179.00, 1),
                (N'Zestaw kamera + speaker USB-C', N'PER-VC-KIT', N'5902000000206', N'Zestaw wideokonferencyjny do sal 4-8 osób.', N'kpl.', N'Peryferia', 820.00, 1199.00, 1),
                (N'Drukarka laserowa mono A4', N'PRN-LSR-MONO-A4', N'5902000000213', N'Drukarka laserowa A4 z duplexem i kartą sieciową.', N'szt.', N'Drukarki i skanery', 720.00, 999.00, 1),
                (N'Skaner dokumentowy A4 duplex', N'SCN-DOC-A4-DPX', N'5902000000220', N'Skaner dokumentowy A4, duplex, automatyczny podajnik ADF.', N'szt.', N'Drukarki i skanery', 940.00, 1299.00, 1),
                (N'Stacja dokująca USB-C 100W', N'ACC-DOCK-USBC100', N'5902000000237', N'Stacja dokująca USB-C z ładowaniem 100 W, HDMI, DP i LAN.', N'szt.', N'Akcesoria komputerowe', 310.00, 469.00, 1),
                (N'Przewód patchcord Cat.6 UTP 2m', N'ACC-PATCH-C6-2M', N'5902000000244', N'Patchcord UTP Cat.6, długość 2 m, opakowanie 10 sztuk.', N'op.', N'Akcesoria komputerowe', 42.00, 69.00, 1)
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
        INSERT (ProductID, Name, SKU, Barcode, Description, UnitOfMeasureID, CategoryID, PurchasePrice, SalePrice, IsActive)
        VALUES (NEWID(), source.Name, source.SKU, source.Barcode, source.Description, source.UnitOfMeasureID, source.CategoryID, source.PurchasePrice, source.SalePrice, source.IsActive);

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
        VALUES (source.UserID, source.Login, source.FirstName, source.LastName, source.Email, @DemoPasswordHash, source.RoleID, source.WarehouseID);

    ---------------------------------------------------------------------------
    -- Stany magazynowe
    -- AvailableQuantity jest kolumną wyliczaną, dlatego nie jest wstawiana.
    ---------------------------------------------------------------------------

    MERGE dbo.Inventory AS target
    USING
    (
        SELECT product.ProductID, location.LocationID, seed.Quantity, seed.ReservedQuantity
        FROM
        (
            VALUES
                (N'NB-BIZ-14512', N'Warszawa - Centrum Dystrybucyjne', N'A-01', CAST(86.000 AS decimal(18,3)), CAST(18.000 AS decimal(18,3))),
                (N'NB-BIZ-14512', N'Poznań - Oddział Zachód', N'PICK-01', 28.000, 7.000),
                (N'NB-BIZ-14512', N'Kraków - Oddział Południe', N'PICK-01', 24.000, 5.000),
                (N'NB-BIZ-14512', N'Gdańsk - Oddział Północ', N'PICK-01', 21.000, 3.000),
                (N'NB-BIZ-151TB', N'Warszawa - Centrum Dystrybucyjne', N'A-01', 54.000, 9.000),
                (N'NB-BIZ-151TB', N'Poznań - Oddział Zachód', N'A-01', 18.000, 4.000),
                (N'PC-WS-I7RTX', N'Warszawa - Centrum Dystrybucyjne', N'A-01', 23.000, 5.000),
                (N'PC-WS-I7RTX', N'Kraków - Oddział Południe', N'A-01', 9.000, 2.000),
                (N'PC-MINI-I5', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 74.000, 16.000),
                (N'PC-MINI-I5', N'Poznań - Oddział Zachód', N'PICK-01', 29.000, 4.000),
                (N'PC-MINI-I5', N'Gdańsk - Oddział Północ', N'PICK-01', 26.000, 5.000),
                (N'MON-27-QHD-USBC', N'Warszawa - Centrum Dystrybucyjne', N'B-01', 160.000, 32.000),
                (N'MON-27-QHD-USBC', N'Poznań - Oddział Zachód', N'A-01', 52.000, 9.000),
                (N'MON-27-QHD-USBC', N'Kraków - Oddział Południe', N'A-01', 47.000, 11.000),
                (N'MON-27-QHD-USBC', N'Gdańsk - Oddział Północ', N'A-01', 38.000, 6.000),
                (N'MON-34-UWQHD', N'Warszawa - Centrum Dystrybucyjne', N'B-01', 42.000, 8.000),
                (N'MON-34-UWQHD', N'Kraków - Oddział Południe', N'A-01', 15.000, 3.000),
                (N'CPU-8C-49BOX', N'Warszawa - Centrum Dystrybucyjne', N'B-01', 122.000, 19.000),
                (N'CPU-8C-49BOX', N'Poznań - Oddział Zachód', N'B-01', 56.000, 11.000),
                (N'MB-B760-ATX-D5', N'Warszawa - Centrum Dystrybucyjne', N'B-01', 92.000, 14.000),
                (N'MB-B760-ATX-D5', N'Gdańsk - Oddział Północ', N'B-01', 35.000, 5.000),
                (N'PSU-750-GOLD', N'Warszawa - Centrum Dystrybucyjne', N'B-01', 118.000, 20.000),
                (N'PSU-750-GOLD', N'Kraków - Oddział Południe', N'B-01', 41.000, 7.000),
                (N'RAM-DDR5-32-5600', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 410.000, 65.000),
                (N'RAM-DDR5-32-5600', N'Poznań - Oddział Zachód', N'PICK-01', 180.000, 28.000),
                (N'RAM-DDR5-32-5600', N'Kraków - Oddział Południe', N'PICK-01', 165.000, 32.000),
                (N'SSD-NVME-1TB-G4', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 540.000, 84.000),
                (N'SSD-NVME-1TB-G4', N'Poznań - Oddział Zachód', N'PICK-01', 210.000, 35.000),
                (N'SSD-NVME-1TB-G4', N'Kraków - Oddział Południe', N'PICK-01', 185.000, 28.000),
                (N'SSD-NVME-1TB-G4', N'Gdańsk - Oddział Północ', N'PICK-01', 175.000, 21.000),
                (N'SSD-NVME-2TB-G4', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 230.000, 40.000),
                (N'SSD-NVME-2TB-G4', N'Gdańsk - Oddział Północ', N'PICK-01', 88.000, 12.000),
                (N'GPU-RTX4070-12', N'Warszawa - Centrum Dystrybucyjne', N'B-01', 38.000, 6.000),
                (N'GPU-RTX4070-12', N'Poznań - Oddział Zachód', N'B-01', 17.000, 3.000),
                (N'GPU-RTX4060-8', N'Kraków - Oddział Południe', N'B-01', 68.000, 12.000),
                (N'GPU-RTX4060-8', N'Gdańsk - Oddział Północ', N'B-01', 55.000, 9.000),
                (N'SRV-1U-XEON', N'Warszawa - Centrum Dystrybucyjne', N'A-01', 14.000, 3.000),
                (N'SRV-1U-XEON', N'Gdańsk - Oddział Północ', N'A-01', 6.000, 1.000),
                (N'NET-SW24-POE', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 105.000, 24.000),
                (N'NET-SW24-POE', N'Kraków - Oddział Południe', N'PICK-01', 62.000, 13.000),
                (N'NET-RTR-VPN-DW', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 82.000, 15.000),
                (N'NET-RTR-VPN-DW', N'Poznań - Oddział Zachód', N'PICK-01', 34.000, 6.000),
                (N'PER-KBD-MECH', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 360.000, 54.000),
                (N'PER-KBD-MECH', N'Poznań - Oddział Zachód', N'PICK-01', 145.000, 24.000),
                (N'PER-MSE-ERG', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 420.000, 55.000),
                (N'PER-MSE-ERG', N'Gdańsk - Oddział Północ', N'PICK-01', 170.000, 25.000),
                (N'PER-VC-KIT', N'Warszawa - Centrum Dystrybucyjne', N'A-01', 44.000, 8.000),
                (N'PER-VC-KIT', N'Kraków - Oddział Południe', N'A-01', 16.000, 2.000),
                (N'PRN-LSR-MONO-A4', N'Warszawa - Centrum Dystrybucyjne', N'A-01', 63.000, 14.000),
                (N'PRN-LSR-MONO-A4', N'Kraków - Oddział Południe', N'PICK-01', 22.000, 4.000),
                (N'SCN-DOC-A4-DPX', N'Warszawa - Centrum Dystrybucyjne', N'A-01', 31.000, 6.000),
                (N'SCN-DOC-A4-DPX', N'Poznań - Oddział Zachód', N'A-01', 12.000, 2.000),
                (N'ACC-DOCK-USBC100', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 260.000, 44.000),
                (N'ACC-DOCK-USBC100', N'Poznań - Oddział Zachód', N'PICK-01', 90.000, 16.000),
                (N'ACC-PATCH-C6-2M', N'Warszawa - Centrum Dystrybucyjne', N'PICK-01', 1250.000, 180.000),
                (N'ACC-PATCH-C6-2M', N'Poznań - Oddział Zachód', N'PICK-01', 620.000, 90.000),
                (N'ACC-PATCH-C6-2M', N'Kraków - Oddział Południe', N'PICK-01', 580.000, 72.000),
                (N'ACC-PATCH-C6-2M', N'Gdańsk - Oddział Północ', N'PICK-01', 490.000, 64.000)
        ) AS seed (SKU, WarehouseName, LocationCode, Quantity, ReservedQuantity)
        INNER JOIN dbo.Products AS product ON product.SKU = seed.SKU
        INNER JOIN dbo.Warehouses AS warehouse ON warehouse.Name = seed.WarehouseName
        INNER JOIN dbo.Locations AS location
            ON location.WarehouseID = warehouse.WarehouseID
            AND location.LocationCode = seed.LocationCode
    ) AS source
        ON target.ProductID = source.ProductID
        AND target.LocationID = source.LocationID
    WHEN MATCHED THEN
        UPDATE SET Quantity = source.Quantity, ReservedQuantity = source.ReservedQuantity
    WHEN NOT MATCHED THEN
        INSERT (InventoryID, ProductID, LocationID, Quantity, ReservedQuantity)
        VALUES (NEWID(), source.ProductID, source.LocationID, source.Quantity, source.ReservedQuantity);

    ---------------------------------------------------------------------------
    -- Wysyłki między magazynami
    -- Status: 1 = oczekuje na akceptację, 2 = gotowa do wysyłki.
    ---------------------------------------------------------------------------

    MERGE dbo.Shipments AS target
    USING
    (
        SELECT
            seed.ShipmentId,
            seed.Number,
            sourceWarehouse.WarehouseID AS SourceWarehouseId,
            destinationWarehouse.WarehouseID AS DestinationWarehouseId,
            createdBy.UserID AS CreatedByUserId,
            seed.CreatedOnUtc,
            approvedBy.UserID AS ApprovedByUserId,
            seed.ApprovedOnUtc,
            seed.Status
        FROM
        (
            VALUES
                (CONVERT(uniqueidentifier, 'b0000000-0000-0000-0000-000000000001'), N'WZ/2026/09/0001', N'Warszawa - Centrum Dystrybucyjne', N'Poznań - Oddział Zachód', N'magazynier.01', CAST(NULL AS nvarchar(100)), CONVERT(datetime2, '2026-09-27T08:15:00'), CAST(NULL AS datetime2), 1),
                (CONVERT(uniqueidentifier, 'b0000000-0000-0000-0000-000000000002'), N'WZ/2026/09/0002', N'Warszawa - Centrum Dystrybucyjne', N'Kraków - Oddział Południe', N'magazynier.01', N'kierownik.waw', CONVERT(datetime2, '2026-09-27T08:40:00'), CONVERT(datetime2, '2026-09-27T09:05:00'), 2),
                (CONVERT(uniqueidentifier, 'b0000000-0000-0000-0000-000000000003'), N'WZ/2026/09/0003', N'Poznań - Oddział Zachód', N'Gdańsk - Oddział Północ', N'operator.02', CAST(NULL AS nvarchar(100)), CONVERT(datetime2, '2026-09-27T10:20:00'), CAST(NULL AS datetime2), 1),
                (CONVERT(uniqueidentifier, 'b0000000-0000-0000-0000-000000000004'), N'WZ/2026/09/0004', N'Gdańsk - Oddział Północ', N'Warszawa - Centrum Dystrybucyjne', N'kierownik.gdn', N'kierownik.gdn', CONVERT(datetime2, '2026-09-27T11:30:00'), CONVERT(datetime2, '2026-09-27T11:45:00'), 2)
        ) AS seed (ShipmentId, Number, SourceWarehouseName, DestinationWarehouseName, CreatedByLogin, ApprovedByLogin, CreatedOnUtc, ApprovedOnUtc, Status)
        INNER JOIN dbo.Warehouses AS sourceWarehouse ON sourceWarehouse.Name = seed.SourceWarehouseName
        INNER JOIN dbo.Warehouses AS destinationWarehouse ON destinationWarehouse.Name = seed.DestinationWarehouseName
        INNER JOIN dbo.Users AS createdBy ON createdBy.Login = seed.CreatedByLogin
        LEFT JOIN dbo.Users AS approvedBy ON approvedBy.Login = seed.ApprovedByLogin
    ) AS source
        ON target.Number = source.Number
    WHEN MATCHED THEN
        UPDATE SET
            SourceWarehouseId = source.SourceWarehouseId,
            DestinationWarehouseId = source.DestinationWarehouseId,
            CreatedByUserId = source.CreatedByUserId,
            CreatedOnUtc = source.CreatedOnUtc,
            ApprovedByUserId = source.ApprovedByUserId,
            ApprovedOnUtc = source.ApprovedOnUtc,
            Status = source.Status
    WHEN NOT MATCHED THEN
        INSERT (ShipmentId, Number, SourceWarehouseId, DestinationWarehouseId, CreatedByUserId, CreatedOnUtc, ApprovedByUserId, ApprovedOnUtc, Status)
        VALUES (source.ShipmentId, source.Number, source.SourceWarehouseId, source.DestinationWarehouseId, source.CreatedByUserId, source.CreatedOnUtc, source.ApprovedByUserId, source.ApprovedOnUtc, source.Status);

    MERGE dbo.ShipmentItems AS target
    USING
    (
        SELECT
            seed.ShipmentItemId,
            shipment.ShipmentId,
            product.ProductID AS ProductId,
            product.Barcode,
            seed.Quantity
        FROM
        (
            VALUES
                (CONVERT(uniqueidentifier, 'c0000000-0000-0000-0000-000000000001'), N'WZ/2026/09/0001', N'NB-BIZ-14512', CAST(8.000 AS decimal(18,3))),
                (CONVERT(uniqueidentifier, 'c0000000-0000-0000-0000-000000000002'), N'WZ/2026/09/0001', N'MON-27-QHD-USBC', 16.000),
                (CONVERT(uniqueidentifier, 'c0000000-0000-0000-0000-000000000003'), N'WZ/2026/09/0001', N'ACC-DOCK-USBC100', 8.000),
                (CONVERT(uniqueidentifier, 'c0000000-0000-0000-0000-000000000004'), N'WZ/2026/09/0002', N'PC-MINI-I5', 12.000),
                (CONVERT(uniqueidentifier, 'c0000000-0000-0000-0000-000000000005'), N'WZ/2026/09/0002', N'RAM-DDR5-32-5600', 24.000),
                (CONVERT(uniqueidentifier, 'c0000000-0000-0000-0000-000000000006'), N'WZ/2026/09/0002', N'SSD-NVME-1TB-G4', 24.000),
                (CONVERT(uniqueidentifier, 'c0000000-0000-0000-0000-000000000007'), N'WZ/2026/09/0003', N'NET-RTR-VPN-DW', 6.000),
                (CONVERT(uniqueidentifier, 'c0000000-0000-0000-0000-000000000008'), N'WZ/2026/09/0003', N'PER-KBD-MECH', 20.000),
                (CONVERT(uniqueidentifier, 'c0000000-0000-0000-0000-000000000009'), N'WZ/2026/09/0004', N'SSD-NVME-2TB-G4', 10.000),
                (CONVERT(uniqueidentifier, 'c0000000-0000-0000-0000-000000000010'), N'WZ/2026/09/0004', N'ACC-PATCH-C6-2M', 40.000)
        ) AS seed (ShipmentItemId, ShipmentNumber, SKU, Quantity)
        INNER JOIN dbo.Shipments AS shipment ON shipment.Number = seed.ShipmentNumber
        INNER JOIN dbo.Products AS product ON product.SKU = seed.SKU
    ) AS source
        ON target.ShipmentId = source.ShipmentId
        AND target.ProductId = source.ProductId
    WHEN MATCHED THEN
        UPDATE SET Barcode = source.Barcode, Quantity = source.Quantity
    WHEN NOT MATCHED THEN
        INSERT (ShipmentItemId, ShipmentId, ProductId, Barcode, Quantity)
        VALUES (source.ShipmentItemId, source.ShipmentId, source.ProductId, source.Barcode, source.Quantity)
    WHEN NOT MATCHED BY SOURCE
        AND target.ShipmentId IN
        (
            CONVERT(uniqueidentifier, 'b0000000-0000-0000-0000-000000000001'),
            CONVERT(uniqueidentifier, 'b0000000-0000-0000-0000-000000000002'),
            CONVERT(uniqueidentifier, 'b0000000-0000-0000-0000-000000000003'),
            CONVERT(uniqueidentifier, 'b0000000-0000-0000-0000-000000000004')
        ) THEN DELETE;

    ---------------------------------------------------------------------------
    -- Historia wysyłek w osobnej bazie MagazineHistory
    ---------------------------------------------------------------------------

    MERGE [MagazineHistory].dbo.ShipmentHistories AS target
    USING
    (
        SELECT
            seed.Id,
            shipment.ShipmentId,
            seed.EventType,
            shipment.SourceWarehouseId,
            shipment.DestinationWarehouseId,
            [user].UserID AS UserId,
            seed.CreatedOnUtc,
            seed.Details
        FROM
        (
            VALUES
                (CONVERT(uniqueidentifier, 'd0000000-0000-0000-0000-000000000001'), N'WZ/2026/09/0001', N'ShipmentCreated', N'magazynier.01', CONVERT(datetime2, '2026-09-27T08:15:00'), N'Utworzono wysyłkę do Poznania: 8 laptopów, 16 monitorów, 8 stacji dokujących.'),
                (CONVERT(uniqueidentifier, 'd0000000-0000-0000-0000-000000000002'), N'WZ/2026/09/0002', N'ShipmentCreated', N'magazynier.01', CONVERT(datetime2, '2026-09-27T08:40:00'), N'Utworzono wysyłkę do Krakowa: mini PC, RAM DDR5 i dyski SSD NVMe.'),
                (CONVERT(uniqueidentifier, 'd0000000-0000-0000-0000-000000000003'), N'WZ/2026/09/0002', N'ShipmentApproved', N'kierownik.waw', CONVERT(datetime2, '2026-09-27T09:05:00'), N'Zaakceptowano wysyłkę i oznaczono ją jako gotową do wydruku WZ oraz etykiety.'),
                (CONVERT(uniqueidentifier, 'd0000000-0000-0000-0000-000000000004'), N'WZ/2026/09/0003', N'ShipmentCreated', N'operator.02', CONVERT(datetime2, '2026-09-27T10:20:00'), N'Utworzono wysyłkę z Poznania do Gdańska: routery VPN oraz klawiatury.'),
                (CONVERT(uniqueidentifier, 'd0000000-0000-0000-0000-000000000005'), N'WZ/2026/09/0004', N'ShipmentCreated', N'kierownik.gdn', CONVERT(datetime2, '2026-09-27T11:30:00'), N'Utworzono wysyłkę z Gdańska do Warszawy: SSD 2 TB oraz patchcordy Cat.6.'),
                (CONVERT(uniqueidentifier, 'd0000000-0000-0000-0000-000000000006'), N'WZ/2026/09/0004', N'ShipmentApproved', N'kierownik.gdn', CONVERT(datetime2, '2026-09-27T11:45:00'), N'Zaakceptowano wysyłkę i przekazano do przygotowania dokumentu WZ oraz etykiety wysyłkowej.')
        ) AS seed (Id, ShipmentNumber, EventType, UserLogin, CreatedOnUtc, Details)
        INNER JOIN dbo.Shipments AS shipment ON shipment.Number = seed.ShipmentNumber
        LEFT JOIN dbo.Users AS [user] ON [user].Login = seed.UserLogin
    ) AS source
        ON target.Id = source.Id
    WHEN MATCHED THEN
        UPDATE SET
            ShipmentId = source.ShipmentId,
            EventType = source.EventType,
            SourceWarehouseId = source.SourceWarehouseId,
            DestinationWarehouseId = source.DestinationWarehouseId,
            UserId = source.UserId,
            CreatedOnUtc = source.CreatedOnUtc,
            Details = source.Details
    WHEN NOT MATCHED THEN
        INSERT (Id, ShipmentId, EventType, SourceWarehouseId, DestinationWarehouseId, UserId, CreatedOnUtc, Details)
        VALUES (source.Id, source.ShipmentId, source.EventType, source.SourceWarehouseId, source.DestinationWarehouseId, source.UserId, source.CreatedOnUtc, source.Details);

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
UNION ALL SELECT N'Inventory', COUNT(*) FROM dbo.Inventory
UNION ALL SELECT N'Shipments', COUNT(*) FROM dbo.Shipments
UNION ALL SELECT N'ShipmentItems', COUNT(*) FROM dbo.ShipmentItems
UNION ALL SELECT N'ShipmentHistories', COUNT(*) FROM [MagazineHistory].dbo.ShipmentHistories;

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

SELECT
    shipment.Number,
    sourceWarehouse.Name AS SourceWarehouse,
    destinationWarehouse.Name AS DestinationWarehouse,
    shipment.Status,
    COUNT(shipmentItem.ShipmentItemId) AS ItemLines,
    SUM(shipmentItem.Quantity) AS TotalQuantity
FROM dbo.Shipments AS shipment
INNER JOIN dbo.Warehouses AS sourceWarehouse
    ON sourceWarehouse.WarehouseID = shipment.SourceWarehouseId
INNER JOIN dbo.Warehouses AS destinationWarehouse
    ON destinationWarehouse.WarehouseID = shipment.DestinationWarehouseId
INNER JOIN dbo.ShipmentItems AS shipmentItem
    ON shipmentItem.ShipmentId = shipment.ShipmentId
GROUP BY
    shipment.Number,
    sourceWarehouse.Name,
    destinationWarehouse.Name,
    shipment.Status
ORDER BY shipment.Number;
GO
