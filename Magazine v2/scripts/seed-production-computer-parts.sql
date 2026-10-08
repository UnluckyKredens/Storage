/*
    Production starter seed for a computer-parts warehouse.

    Run this script against the target application database after EF migrations.
    It intentionally does not create users or test passwords. The first
    administrator is handled by MagazineAPI InitialAdmin configuration.

    Example:
    sqlcmd -S tcp:YOUR_SQL_SERVER,1433 -d Magazine -U YOUR_USER -P YOUR_PASSWORD -i scripts/seed-production-computer-parts.sql
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;

BEGIN TRY
    BEGIN TRANSACTION

    DECLARE @Roles TABLE
    (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        Name nvarchar(50) NOT NULL
    );

    INSERT INTO @Roles (Id, Name)
    VALUES
        ('10000000-0000-0000-0000-000000000001', N'Administrator'),
        ('10000000-0000-0000-0000-000000000002', N'Kierownik'),
        ('10000000-0000-0000-0000-000000000003', N'Pracownik');

    MERGE dbo.Roles AS target
    USING @Roles AS source
        ON target.Id = source.Id
    WHEN MATCHED THEN
        UPDATE SET Name = source.Name
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (Id, Name)
        VALUES (source.Id, source.Name);

    DECLARE @Permissions TABLE
    (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        Code nvarchar(100) NOT NULL,
        Name nvarchar(150) NOT NULL,
        Description nvarchar(500) NULL
    );

    INSERT INTO @Permissions (Id, Code, Name, Description)
    VALUES
        ('20000000-0000-0000-0000-000000000001', N'products.read', N'Podglad produktow', N'Wyswietlanie katalogu i szczegolow produktow.'),
        ('20000000-0000-0000-0000-000000000002', N'products.manage', N'Zarzadzanie produktami', N'Dodawanie, edycja i wycofywanie produktow.'),
        ('20000000-0000-0000-0000-000000000003', N'inventory.read', N'Podglad stanow', N'Wyswietlanie stanow i rezerwacji magazynowych.'),
        ('20000000-0000-0000-0000-000000000004', N'inventory.manage', N'Zarzadzanie stanami', N'Bezposrednia edycja stanow magazynowych.'),
        ('20000000-0000-0000-0000-000000000005', N'warehouses.read', N'Podglad magazynow', N'Wyswietlanie magazynow oraz lokalizacji.'),
        ('20000000-0000-0000-0000-000000000006', N'warehouses.manage', N'Zarzadzanie magazynami', N'Dodawanie i edycja magazynow oraz lokalizacji.'),
        ('20000000-0000-0000-0000-000000000007', N'contractors.read', N'Podglad kontrahentow', N'Wyswietlanie dostawcow i odbiorcow.'),
        ('20000000-0000-0000-0000-000000000008', N'contractors.manage', N'Zarzadzanie kontrahentami', N'Dodawanie i edycja kontrahentow.'),
        ('20000000-0000-0000-0000-000000000009', N'users.read', N'Podglad uzytkownikow', N'Wyswietlanie kont uzytkownikow.'),
        ('20000000-0000-0000-0000-000000000010', N'users.manage', N'Zarzadzanie uzytkownikami', N'Edycja i usuwanie kont innych niz administratorzy.'),
        ('20000000-0000-0000-0000-000000000011', N'dictionaries.manage', N'Zarzadzanie slownikami', N'Edycja kategorii i jednostek miary.'),
        ('20000000-0000-0000-0000-000000000012', N'shipments.read', N'Podglad wysylek', N'Wyswietlanie wysylek miedzy magazynami.'),
        ('20000000-0000-0000-0000-000000000013', N'shipments.create', N'Tworzenie wysylek', N'Tworzenie wysylek z magazynu pracownika.'),
        ('20000000-0000-0000-0000-000000000014', N'shipments.approve', N'Akceptacja wysylek', N'Akceptowanie wysylek i przygotowanie WZ.'),
        ('20000000-0000-0000-0000-000000000015', N'purchase-orders.read', N'Podglad zamowien zewnetrznych', N'Wyswietlanie zamowien do magazynu od dostawcow.'),
        ('20000000-0000-0000-0000-000000000016', N'purchase-orders.create', N'Tworzenie zamowien zewnetrznych', N'Tworzenie zamowien do aktywnego magazynu.'),
        ('20000000-0000-0000-0000-000000000017', N'purchase-orders.approve', N'Akceptacja zamowien zewnetrznych', N'Akceptowanie zamowien, faktur i dokumentow przyjecia.');

    MERGE dbo.Permissions AS target
    USING @Permissions AS source
        ON target.Id = source.Id
    WHEN MATCHED THEN
        UPDATE SET
            Code = source.Code,
            Name = source.Name,
            Description = source.Description
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (Id, Code, Name, Description)
        VALUES (source.Id, source.Code, source.Name, source.Description);

    DECLARE @RolePermissions TABLE
    (
        RoleId uniqueidentifier NOT NULL,
        PermissionId uniqueidentifier NOT NULL,
        PRIMARY KEY (RoleId, PermissionId)
    );

    INSERT INTO @RolePermissions (RoleId, PermissionId)
    VALUES
        ('10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000001'),
        ('10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000002'),
        ('10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000003'),
        ('10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000004'),
        ('10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000005'),
        ('10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000007'),
        ('10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000012'),
        ('10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000013'),
        ('10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000014'),
        ('10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000015'),
        ('10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000016'),
        ('10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000017'),
        ('10000000-0000-0000-0000-000000000003', '20000000-0000-0000-0000-000000000001'),
        ('10000000-0000-0000-0000-000000000003', '20000000-0000-0000-0000-000000000003'),
        ('10000000-0000-0000-0000-000000000003', '20000000-0000-0000-0000-000000000012'),
        ('10000000-0000-0000-0000-000000000003', '20000000-0000-0000-0000-000000000013'),
        ('10000000-0000-0000-0000-000000000003', '20000000-0000-0000-0000-000000000015'),
        ('10000000-0000-0000-0000-000000000003', '20000000-0000-0000-0000-000000000016');

    MERGE dbo.RolePermissions AS target
    USING @RolePermissions AS source
        ON target.RoleId = source.RoleId
        AND target.PermissionId = source.PermissionId
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (RoleId, PermissionId)
        VALUES (source.RoleId, source.PermissionId);

    DECLARE @Units TABLE
    (
        UnitOfMeasureID uniqueidentifier NOT NULL PRIMARY KEY,
        Name nvarchar(100) NOT NULL,
        Symbol nvarchar(20) NOT NULL
    );

    INSERT INTO @Units (UnitOfMeasureID, Name, Symbol)
    VALUES
        ('30000000-0000-0000-0000-000000000001', N'Sztuka', N'szt.'),
        ('30000000-0000-0000-0000-000000000002', N'Opakowanie', N'op.'),
        ('30000000-0000-0000-0000-000000000003', N'Komplet', N'kpl.');

    MERGE dbo.UnitsOfMeasure AS target
    USING @Units AS source
        ON target.Symbol = source.Symbol
    WHEN MATCHED THEN
        UPDATE SET Name = source.Name
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (UnitOfMeasureID, Name, Symbol)
        VALUES (source.UnitOfMeasureID, source.Name, source.Symbol);

    DECLARE @Categories TABLE
    (
        CategoryID uniqueidentifier NOT NULL PRIMARY KEY,
        Name nvarchar(200) NOT NULL,
        Description nvarchar(1000) NULL
    );

    INSERT INTO @Categories (CategoryID, Name, Description)
    VALUES
        ('31000000-0000-0000-0000-000000000001', N'Procesory', N'CPU desktopowe Intel i AMD.'),
        ('31000000-0000-0000-0000-000000000002', N'Plyty glowne', N'Plyty glowne dla platform Intel i AMD.'),
        ('31000000-0000-0000-0000-000000000003', N'Pamiec RAM', N'Moduly DDR4 i DDR5.'),
        ('31000000-0000-0000-0000-000000000004', N'Dyski SSD', N'Dyski NVMe i SATA.'),
        ('31000000-0000-0000-0000-000000000005', N'Karty graficzne', N'Karty GPU dla stacji roboczych i komputerow gamingowych.'),
        ('31000000-0000-0000-0000-000000000006', N'Zasilacze', N'Zasilacze ATX o roznych mocach.'),
        ('31000000-0000-0000-0000-000000000007', N'Obudowy', N'Obudowy komputerowe.'),
        ('31000000-0000-0000-0000-000000000008', N'Chlodzenie', N'Chlodzenia powietrzne, AIO i akcesoria termiczne.'),
        ('31000000-0000-0000-0000-000000000009', N'Siec', N'Urzadzenia sieciowe i akcesoria.'),
        ('31000000-0000-0000-0000-000000000010', N'Peryferia', N'Klawiatury, myszy i akcesoria stanowiskowe.'),
        ('31000000-0000-0000-0000-000000000011', N'Monitory', N'Monitory biurowe i gamingowe.'),
        ('31000000-0000-0000-0000-000000000012', N'Akcesoria', N'Kable, pasty, maty i drobne wyposazenie serwisowe.');

    UPDATE target
    SET Description = source.Description
    FROM dbo.Categories AS target
    INNER JOIN @Categories AS source
        ON source.Name = target.Name;

    INSERT INTO dbo.Categories (CategoryID, Name, Description)
    SELECT source.CategoryID, source.Name, source.Description
    FROM @Categories AS source
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Categories AS target
        WHERE target.Name = source.Name
    );

    DECLARE @Contractors TABLE
    (
        ContractorID uniqueidentifier NOT NULL PRIMARY KEY,
        Name nvarchar(200) NOT NULL,
        TaxNumber nvarchar(50) NOT NULL,
        Type nvarchar(20) NOT NULL,
        Email nvarchar(255) NULL,
        Phone nvarchar(50) NULL,
        Address nvarchar(500) NULL
    );

    INSERT INTO @Contractors (ContractorID, Name, TaxNumber, Type, Email, Phone, Address)
    VALUES
        ('32000000-0000-0000-0000-000000000001', N'ABC Data Components sp. z o.o.', N'5250001001', N'Supplier', N'b2b@abc-components.pl', N'+48 22 100 10 01', N'ul. Modularna 7, 02-238 Warszawa'),
        ('32000000-0000-0000-0000-000000000002', N'Komputronik Biznes sp. z o.o.', N'7780002002', N'Both', N'hurt@komputronik-biznes.pl', N'+48 61 200 20 02', N'ul. Handlowa 18, 60-166 Poznan'),
        ('32000000-0000-0000-0000-000000000003', N'Incom Group S.A.', N'8940003003', N'Supplier', N'orders@incom.pl', N'+48 71 300 30 03', N'ul. Informatyczna 12, 54-105 Wroclaw'),
        ('32000000-0000-0000-0000-000000000004', N'Also Polska sp. z o.o.', N'5270004004', N'Supplier', N'sprzedaz@also.pl', N'+48 22 400 40 04', N'ul. Dystrybucyjna 3, 05-500 Piaseczno'),
        ('32000000-0000-0000-0000-000000000005', N'Morele Business Center', N'6760005005', N'Both', N'b2b@morele.net', N'+48 12 500 50 05', N'al. Technologiczna 22, 31-864 Krakow'),
        ('32000000-0000-0000-0000-000000000006', N'PC Service Partner', N'8990006006', N'Customer', N'serwis@pc-partner.pl', N'+48 71 600 60 06', N'ul. Serwisowa 9, 53-609 Wroclaw'),
        ('32000000-0000-0000-0000-000000000007', N'Office IT Solutions', N'5210007007', N'Customer', N'zakupy@office-it.pl', N'+48 22 700 70 07', N'ul. Biurowa 41, 00-950 Warszawa'),
        ('32000000-0000-0000-0000-000000000008', N'Gaming Zone Retail', N'9450008008', N'Customer', N'magazyn@gamingzone.pl', N'+48 12 800 80 08', N'ul. Graczy 5, 30-701 Krakow');

    UPDATE target
    SET
        TaxNumber = source.TaxNumber,
        Type = source.Type,
        Email = source.Email,
        Phone = source.Phone,
        Address = source.Address
    FROM dbo.Contractors AS target
    INNER JOIN @Contractors AS source
        ON source.Name = target.Name;

    INSERT INTO dbo.Contractors (ContractorID, Name, TaxNumber, Type, Email, Phone, Address)
    SELECT source.ContractorID, source.Name, source.TaxNumber, source.Type, source.Email, source.Phone, source.Address
    FROM @Contractors AS source
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Contractors AS target
        WHERE target.Name = source.Name
    );

    DECLARE @Warehouses TABLE
    (
        WarehouseID uniqueidentifier NOT NULL PRIMARY KEY,
        Name nvarchar(200) NOT NULL,
        Address nvarchar(500) NULL,
        Description nvarchar(1000) NULL
    );

    INSERT INTO @Warehouses (WarehouseID, Name, Address, Description)
    VALUES
        ('33000000-0000-0000-0000-000000000001', N'MAG-01 Centrum Komponentow', N'ul. Logistyczna 12, 05-850 Ozarow Mazowiecki', N'Glowny magazyn czesci komputerowych i kompletacji zamowien.'),
        ('33000000-0000-0000-0000-000000000002', N'MAG-02 Serwis i Zwroty', N'ul. Serwisowa 4, 05-850 Ozarow Mazowiecki', N'Magazyn reklamacji, zwrotow i kontroli jakosci.');

    UPDATE target
    SET
        Address = source.Address,
        Description = source.Description
    FROM dbo.Warehouses AS target
    INNER JOIN @Warehouses AS source
        ON source.Name = target.Name;

    INSERT INTO dbo.Warehouses (WarehouseID, Name, Address, Description)
    SELECT source.WarehouseID, source.Name, source.Address, source.Description
    FROM @Warehouses AS source
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Warehouses AS target
        WHERE target.Name = source.Name
    );

    DECLARE @Locations TABLE
    (
        LocationID uniqueidentifier NOT NULL PRIMARY KEY,
        WarehouseName nvarchar(200) NOT NULL,
        LocationCode nvarchar(50) NOT NULL,
        Description nvarchar(500) NULL
    );

    INSERT INTO @Locations (LocationID, WarehouseName, LocationCode, Description)
    VALUES
        ('34000000-0000-0000-0000-000000000001', N'MAG-01 Centrum Komponentow', N'REC-01', N'Przyjecie dostaw i kontrola ilosciowa.'),
        ('34000000-0000-0000-0000-000000000002', N'MAG-01 Centrum Komponentow', N'A-CPU', N'Regal procesorow i plyt glownych.'),
        ('34000000-0000-0000-0000-000000000003', N'MAG-01 Centrum Komponentow', N'A-RAM', N'Regal pamieci RAM.'),
        ('34000000-0000-0000-0000-000000000004', N'MAG-01 Centrum Komponentow', N'A-SSD', N'Regal dyskow SSD.'),
        ('34000000-0000-0000-0000-000000000005', N'MAG-01 Centrum Komponentow', N'A-GPU', N'Strefa kart graficznych.'),
        ('34000000-0000-0000-0000-000000000006', N'MAG-01 Centrum Komponentow', N'B-PSU', N'Regal zasilaczy i obudow.'),
        ('34000000-0000-0000-0000-000000000007', N'MAG-01 Centrum Komponentow', N'C-NET', N'Strefa sieci i peryferii.'),
        ('34000000-0000-0000-0000-000000000008', N'MAG-01 Centrum Komponentow', N'PICK-01', N'Kompletacja zamowien sklepowych.'),
        ('34000000-0000-0000-0000-000000000009', N'MAG-02 Serwis i Zwroty', N'QC-01', N'Kontrola jakosci po zwrotach.'),
        ('34000000-0000-0000-0000-000000000010', N'MAG-02 Serwis i Zwroty', N'RET-01', N'Bufor zwrotow i reklamacji.');

    ;WITH LocationSource AS
    (
        SELECT
            warehouses.WarehouseID,
            locations.LocationID,
            locations.LocationCode,
            locations.Description
        FROM @Locations AS locations
        INNER JOIN dbo.Warehouses AS warehouses
            ON warehouses.Name = locations.WarehouseName
    )
    MERGE dbo.Locations AS target
    USING LocationSource AS source
        ON target.WarehouseID = source.WarehouseID
        AND target.LocationCode = source.LocationCode
    WHEN MATCHED THEN
        UPDATE SET Description = source.Description
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (LocationID, WarehouseID, LocationCode, Description)
        VALUES (source.LocationID, source.WarehouseID, source.LocationCode, source.Description);

    DECLARE @Products TABLE
    (
        ProductID uniqueidentifier NOT NULL PRIMARY KEY,
        SKU nvarchar(100) NOT NULL UNIQUE,
        Name nvarchar(200) NOT NULL,
        Barcode nvarchar(100) NULL,
        CategoryName nvarchar(200) NOT NULL,
        UnitSymbol nvarchar(20) NOT NULL,
        PurchasePrice decimal(18, 2) NOT NULL,
        SalePrice decimal(18, 2) NOT NULL,
        MinimumQuantity decimal(18, 3) NOT NULL,
        OptimumQuantity decimal(18, 3) NULL,
        ImageUrl nvarchar(1000) NULL,
        Description nvarchar(1000) NULL
    );

    INSERT INTO @Products
        (ProductID, SKU, Name, Barcode, CategoryName, UnitSymbol, PurchasePrice, SalePrice, MinimumQuantity, OptimumQuantity, ImageUrl, Description)
    VALUES
        ('35000000-0000-0000-0000-000000000001', N'CPU-INT-14700K', N'Intel Core i7-14700K', N'5901000000011', N'Procesory', N'szt.', 1650.00, 1999.00, 4, 12, N'https://images.unsplash.com/photo-1591799264318-7e6ef8ddb7ea?auto=format&fit=crop&w=900&q=80', N'Procesor Intel Core i7 do stacji roboczych i zestawow gamingowych.'),
        ('35000000-0000-0000-0000-000000000002', N'CPU-AMD-7800X3D', N'AMD Ryzen 7 7800X3D', N'5901000000028', N'Procesory', N'szt.', 1450.00, 1799.00, 4, 12, N'https://images.unsplash.com/photo-1555618254-8aefccddc793?auto=format&fit=crop&w=900&q=80', N'Procesor AMD Ryzen z pamiecia 3D V-Cache do wydajnych komputerow.'),
        ('35000000-0000-0000-0000-000000000003', N'CPU-AMD-5600', N'AMD Ryzen 5 5600', N'5901000000035', N'Procesory', N'szt.', 430.00, 599.00, 8, 24, N'https://images.unsplash.com/photo-1592664474496-8f59f9c97048?auto=format&fit=crop&w=900&q=80', N'Popularny procesor AM4 do zestawow biurowych i domowych.'),
        ('35000000-0000-0000-0000-000000000004', N'MB-ASUS-B650-PLUS', N'ASUS TUF Gaming B650-PLUS', N'5901000000042', N'Plyty glowne', N'szt.', 690.00, 899.00, 3, 10, N'https://images.unsplash.com/photo-1518770660439-4636190af475?auto=format&fit=crop&w=900&q=80', N'Plyta glowna AM5 z obsluga DDR5 i PCIe 4.0.'),
        ('35000000-0000-0000-0000-000000000005', N'MB-MSI-Z790-P', N'MSI PRO Z790-P WiFi', N'5901000000059', N'Plyty glowne', N'szt.', 820.00, 1099.00, 3, 10, N'https://images.unsplash.com/photo-1562976540-1502c2145186?auto=format&fit=crop&w=900&q=80', N'Plyta glowna LGA1700 z WiFi dla procesorow Intel.'),
        ('35000000-0000-0000-0000-000000000006', N'MB-GIGA-B550-DS3H', N'Gigabyte B550M DS3H', N'5901000000066', N'Plyty glowne', N'szt.', 310.00, 459.00, 5, 15, N'https://images.unsplash.com/photo-1587202372775-e229f172b9d7?auto=format&fit=crop&w=900&q=80', N'Micro ATX AM4 do ekonomicznych zestawow komputerowych.'),
        ('35000000-0000-0000-0000-000000000007', N'RAM-KING-32-DDR5-6000', N'Kingston Fury Beast 32GB DDR5 6000', N'5901000000073', N'Pamiec RAM', N'kpl.', 360.00, 499.00, 10, 30, N'https://images.unsplash.com/photo-1562408590-e32931084e23?auto=format&fit=crop&w=900&q=80', N'Zestaw 2x16GB DDR5 dla platform Intel i AMD.'),
        ('35000000-0000-0000-0000-000000000008', N'RAM-COR-16-DDR4-3200', N'Corsair Vengeance LPX 16GB DDR4 3200', N'5901000000080', N'Pamiec RAM', N'kpl.', 130.00, 219.00, 15, 45, N'https://images.unsplash.com/photo-1591488320449-011701bb6704?auto=format&fit=crop&w=900&q=80', N'Zestaw 2x8GB DDR4 do komputerow biurowych i domowych.'),
        ('35000000-0000-0000-0000-000000000009', N'RAM-GSKILL-64-DDR5-5600', N'G.Skill Ripjaws S5 64GB DDR5 5600', N'5901000000097', N'Pamiec RAM', N'kpl.', 720.00, 949.00, 4, 12, N'https://images.unsplash.com/photo-1555617981-dac3880eac6e?auto=format&fit=crop&w=900&q=80', N'Zestaw 2x32GB DDR5 do pracy kreatywnej i wirtualizacji.'),
        ('35000000-0000-0000-0000-000000000010', N'SSD-SAM-990PRO-1TB', N'Samsung 990 PRO 1TB NVMe', N'5901000000103', N'Dyski SSD', N'szt.', 330.00, 469.00, 12, 36, N'https://images.unsplash.com/photo-1597872200969-2b65d56bd16b?auto=format&fit=crop&w=900&q=80', N'Szybki dysk NVMe PCIe 4.0 do systemow i gier.'),
        ('35000000-0000-0000-0000-000000000011', N'SSD-WD-SN770-2TB', N'WD Black SN770 2TB NVMe', N'5901000000110', N'Dyski SSD', N'szt.', 430.00, 649.00, 8, 24, N'https://images.unsplash.com/photo-1617531653332-bd46c24f2068?auto=format&fit=crop&w=900&q=80', N'Dysk NVMe 2TB do zestawow gamingowych i roboczych.'),
        ('35000000-0000-0000-0000-000000000012', N'SSD-KIOXIA-EXCERIA-1TB', N'Kioxia Exceria G2 1TB NVMe', N'5901000000127', N'Dyski SSD', N'szt.', 210.00, 329.00, 10, 30, N'https://images.unsplash.com/photo-1601737487795-dab272f52420?auto=format&fit=crop&w=900&q=80', N'Ekonomiczny dysk NVMe do komputerow biurowych.'),
        ('35000000-0000-0000-0000-000000000013', N'GPU-ASUS-RTX4070S-12', N'ASUS Dual GeForce RTX 4070 SUPER 12GB', N'5901000000134', N'Karty graficzne', N'szt.', 2550.00, 3199.00, 2, 8, N'https://images.unsplash.com/photo-1591405351990-4726e331f141?auto=format&fit=crop&w=900&q=80', N'Karta graficzna NVIDIA do gier i akceleracji AI.'),
        ('35000000-0000-0000-0000-000000000014', N'GPU-SAPPHIRE-RX7800XT-16', N'Sapphire Pulse Radeon RX 7800 XT 16GB', N'5901000000141', N'Karty graficzne', N'szt.', 2050.00, 2599.00, 2, 8, N'https://images.unsplash.com/photo-1587202372634-32705e3bf49c?auto=format&fit=crop&w=900&q=80', N'Karta graficzna AMD z 16GB pamieci VRAM.'),
        ('35000000-0000-0000-0000-000000000015', N'GPU-GIGA-RTX4060-8', N'Gigabyte GeForce RTX 4060 Windforce 8GB', N'5901000000158', N'Karty graficzne', N'szt.', 1180.00, 1549.00, 4, 12, N'https://images.unsplash.com/photo-1591488320449-011701bb6704?auto=format&fit=crop&w=900&q=80', N'Karta graficzna do kompaktowych zestawow gamingowych.'),
        ('35000000-0000-0000-0000-000000000016', N'PSU-SEASONIC-750-GX', N'Seasonic Focus GX 750W Gold', N'5901000000165', N'Zasilacze', N'szt.', 420.00, 599.00, 5, 18, N'https://images.unsplash.com/photo-1624705013726-8cb4f9415f40?auto=format&fit=crop&w=900&q=80', N'Modularny zasilacz 80 Plus Gold do wydajnych komputerow.'),
        ('35000000-0000-0000-0000-000000000017', N'PSU-CORSAIR-850E', N'Corsair RM850e 850W Gold', N'5901000000172', N'Zasilacze', N'szt.', 470.00, 679.00, 4, 16, N'https://images.unsplash.com/photo-1616588589676-62b3bd4ff6d2?auto=format&fit=crop&w=900&q=80', N'Zasilacz ATX 3.0 do konfiguracji z mocniejsza karta graficzna.'),
        ('35000000-0000-0000-0000-000000000018', N'CASE-FRACTAL-POP-AIR', N'Fractal Design Pop Air Black', N'5901000000189', N'Obudowy', N'szt.', 285.00, 429.00, 4, 16, N'https://images.unsplash.com/photo-1587202372616-b43abea06c2a?auto=format&fit=crop&w=900&q=80', N'Przewiewna obudowa ATX z miejscem na rozbudowe.'),
        ('35000000-0000-0000-0000-000000000019', N'CASE-NZXT-H5-FLOW', N'NZXT H5 Flow White', N'5901000000196', N'Obudowy', N'szt.', 340.00, 519.00, 3, 12, N'https://images.unsplash.com/photo-1624705013726-8cb4f9415f40?auto=format&fit=crop&w=900&q=80', N'Obudowa ATX o minimalistycznym wygladzie i dobrym przeplywie powietrza.'),
        ('35000000-0000-0000-0000-000000000020', N'COOL-NOCTUA-NH-D15', N'Noctua NH-D15 chromax.black', N'5901000000202', N'Chlodzenie', N'szt.', 430.00, 599.00, 3, 10, N'https://images.unsplash.com/photo-1587202372775-e229f172b9d7?auto=format&fit=crop&w=900&q=80', N'Wydajne chlodzenie powietrzne dla procesorow klasy premium.'),
        ('35000000-0000-0000-0000-000000000021', N'COOL-ARCTIC-LF-III-360', N'Arctic Liquid Freezer III 360', N'5901000000219', N'Chlodzenie', N'szt.', 390.00, 549.00, 3, 10, N'https://images.unsplash.com/photo-1591799264318-7e6ef8ddb7ea?auto=format&fit=crop&w=900&q=80', N'Chlodzenie wodne AIO 360 mm do mocnych zestawow.'),
        ('35000000-0000-0000-0000-000000000022', N'NET-TP-SG108', N'TP-Link TL-SG108 switch 8-port', N'5901000000226', N'Siec', N'szt.', 85.00, 139.00, 10, 30, N'https://images.unsplash.com/photo-1544197150-b99a580bb7a8?auto=format&fit=crop&w=900&q=80', N'Nie zarzadzany switch gigabitowy do biura i domu.'),
        ('35000000-0000-0000-0000-000000000023', N'NET-UBI-U6-LITE', N'Ubiquiti UniFi U6 Lite', N'5901000000233', N'Siec', N'szt.', 360.00, 499.00, 4, 12, N'https://images.unsplash.com/photo-1600267175161-cfaa711b4a81?auto=format&fit=crop&w=900&q=80', N'Punkt dostepowy WiFi 6 do instalacji firmowych.'),
        ('35000000-0000-0000-0000-000000000024', N'KB-LOGI-MX-KEYS', N'Logitech MX Keys', N'5901000000240', N'Peryferia', N'szt.', 290.00, 429.00, 5, 18, N'https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&w=900&q=80', N'Bezprzewodowa klawiatura niskoprofilowa do pracy biurowej.'),
        ('35000000-0000-0000-0000-000000000025', N'MOUSE-LOGI-MX-MASTER-3S', N'Logitech MX Master 3S', N'5901000000257', N'Peryferia', N'szt.', 270.00, 399.00, 5, 18, N'https://images.unsplash.com/photo-1527814050087-3793815479db?auto=format&fit=crop&w=900&q=80', N'Ergonomiczna mysz bezprzewodowa dla stanowisk roboczych.'),
        ('35000000-0000-0000-0000-000000000026', N'MON-DELL-U2723QE', N'Dell UltraSharp U2723QE 27 4K', N'5901000000264', N'Monitory', N'szt.', 1890.00, 2399.00, 2, 8, N'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=900&q=80', N'Monitor 4K USB-C do pracy biurowej i kreatywnej.'),
        ('35000000-0000-0000-0000-000000000027', N'MON-LG-27GP850', N'LG UltraGear 27GP850 27 QHD', N'5901000000271', N'Monitory', N'szt.', 1090.00, 1499.00, 3, 10, N'https://images.unsplash.com/photo-1547082299-de196ea013d6?auto=format&fit=crop&w=900&q=80', N'Monitor QHD 165 Hz do stanowisk gamingowych.'),
        ('35000000-0000-0000-0000-000000000028', N'ACC-PASTE-MX6-4G', N'Arctic MX-6 pasta termiczna 4g', N'5901000000288', N'Akcesoria', N'szt.', 18.00, 39.00, 25, 80, N'https://images.unsplash.com/photo-1612815154858-60aa4c59eaa6?auto=format&fit=crop&w=900&q=80', N'Pasta termiczna do montazu i serwisu chlodzenia CPU/GPU.'),
        ('35000000-0000-0000-0000-000000000029', N'ACC-CABLE-DP14-2M', N'Kabel DisplayPort 1.4 2m', N'5901000000295', N'Akcesoria', N'szt.', 22.00, 49.00, 20, 80, N'https://images.unsplash.com/photo-1609250291996-fdebe6020a8f?auto=format&fit=crop&w=900&q=80', N'Kabel DisplayPort do monitorow QHD i 4K.'),
        ('35000000-0000-0000-0000-000000000030', N'TOOL-ESD-MAT', N'Mata serwisowa ESD 60x40 cm', N'5901000000301', N'Akcesoria', N'szt.', 42.00, 89.00, 8, 24, N'https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=900&q=80', N'Mata antystatyczna do pracy przy podzespolach komputerowych.');

    ;WITH ProductSource AS
    (
        SELECT
            products.SKU,
            products.ProductID,
            products.Name,
            products.Barcode,
            products.Description,
            products.ImageUrl,
            units.UnitOfMeasureID,
            categories.CategoryID,
            products.PurchasePrice,
            products.SalePrice,
            products.MinimumQuantity,
            products.OptimumQuantity
        FROM @Products AS products
        INNER JOIN dbo.UnitsOfMeasure AS units
            ON units.Symbol = products.UnitSymbol
        INNER JOIN dbo.Categories AS categories
            ON categories.Name = products.CategoryName
    )
    MERGE dbo.Products AS target
    USING ProductSource AS source
        ON target.SKU = source.SKU
    WHEN MATCHED THEN
        UPDATE SET
            Name = source.Name,
            Barcode = source.Barcode,
            Description = source.Description,
            ImageUrl = source.ImageUrl,
            UnitOfMeasureID = source.UnitOfMeasureID,
            CategoryID = source.CategoryID,
            PurchasePrice = source.PurchasePrice,
            SalePrice = source.SalePrice,
            MinimumQuantity = source.MinimumQuantity,
            OptimumQuantity = source.OptimumQuantity,
            IsActive = 1
    WHEN NOT MATCHED BY TARGET THEN
        INSERT
        (
            Name,
            SKU,
            ProductID,
            Barcode,
            Description,
            ImageUrl,
            UnitOfMeasureID,
            CategoryID,
            PurchasePrice,
            SalePrice,
            MinimumQuantity,
            OptimumQuantity,
            IsActive
        )
        VALUES
        (
            source.Name,
            source.SKU,
            source.ProductID,
            source.Barcode,
            source.Description,
            source.ImageUrl,
            source.UnitOfMeasureID,
            source.CategoryID,
            source.PurchasePrice,
            source.SalePrice,
            source.MinimumQuantity,
            source.OptimumQuantity,
            1
        );

    DECLARE @Inventory TABLE
    (
        InventoryID uniqueidentifier NOT NULL PRIMARY KEY,
        SKU nvarchar(100) NOT NULL,
        WarehouseName nvarchar(200) NOT NULL,
        LocationCode nvarchar(50) NOT NULL,
        Quantity decimal(18, 3) NOT NULL,
        ReservedQuantity decimal(18, 3) NOT NULL
    );

    INSERT INTO @Inventory (InventoryID, SKU, WarehouseName, LocationCode, Quantity, ReservedQuantity)
    VALUES
        ('36000000-0000-0000-0000-000000000001', N'CPU-INT-14700K', N'MAG-01 Centrum Komponentow', N'A-CPU', 9, 0),
        ('36000000-0000-0000-0000-000000000002', N'CPU-AMD-7800X3D', N'MAG-01 Centrum Komponentow', N'A-CPU', 8, 0),
        ('36000000-0000-0000-0000-000000000003', N'CPU-AMD-5600', N'MAG-01 Centrum Komponentow', N'A-CPU', 24, 0),
        ('36000000-0000-0000-0000-000000000004', N'MB-ASUS-B650-PLUS', N'MAG-01 Centrum Komponentow', N'A-CPU', 11, 0),
        ('36000000-0000-0000-0000-000000000005', N'MB-MSI-Z790-P', N'MAG-01 Centrum Komponentow', N'A-CPU', 9, 0),
        ('36000000-0000-0000-0000-000000000006', N'MB-GIGA-B550-DS3H', N'MAG-01 Centrum Komponentow', N'A-CPU', 18, 0),
        ('36000000-0000-0000-0000-000000000007', N'RAM-KING-32-DDR5-6000', N'MAG-01 Centrum Komponentow', N'A-RAM', 34, 0),
        ('36000000-0000-0000-0000-000000000008', N'RAM-COR-16-DDR4-3200', N'MAG-01 Centrum Komponentow', N'A-RAM', 50, 0),
        ('36000000-0000-0000-0000-000000000009', N'RAM-GSKILL-64-DDR5-5600', N'MAG-01 Centrum Komponentow', N'A-RAM', 13, 0),
        ('36000000-0000-0000-0000-000000000010', N'SSD-SAM-990PRO-1TB', N'MAG-01 Centrum Komponentow', N'A-SSD', 39, 0),
        ('36000000-0000-0000-0000-000000000011', N'SSD-WD-SN770-2TB', N'MAG-01 Centrum Komponentow', N'A-SSD', 27, 0),
        ('36000000-0000-0000-0000-000000000012', N'SSD-KIOXIA-EXCERIA-1TB', N'MAG-01 Centrum Komponentow', N'A-SSD', 34, 0),
        ('36000000-0000-0000-0000-000000000013', N'GPU-ASUS-RTX4070S-12', N'MAG-01 Centrum Komponentow', N'A-GPU', 7, 0),
        ('36000000-0000-0000-0000-000000000014', N'GPU-SAPPHIRE-RX7800XT-16', N'MAG-01 Centrum Komponentow', N'A-GPU', 8, 0),
        ('36000000-0000-0000-0000-000000000015', N'GPU-GIGA-RTX4060-8', N'MAG-01 Centrum Komponentow', N'A-GPU', 15, 0),
        ('36000000-0000-0000-0000-000000000016', N'PSU-SEASONIC-750-GX', N'MAG-01 Centrum Komponentow', N'B-PSU', 19, 0),
        ('36000000-0000-0000-0000-000000000017', N'PSU-CORSAIR-850E', N'MAG-01 Centrum Komponentow', N'B-PSU', 17, 0),
        ('36000000-0000-0000-0000-000000000018', N'CASE-FRACTAL-POP-AIR', N'MAG-01 Centrum Komponentow', N'B-PSU', 18, 0),
        ('36000000-0000-0000-0000-000000000019', N'CASE-NZXT-H5-FLOW', N'MAG-01 Centrum Komponentow', N'B-PSU', 13, 0),
        ('36000000-0000-0000-0000-000000000020', N'COOL-NOCTUA-NH-D15', N'MAG-01 Centrum Komponentow', N'B-PSU', 12, 0),
        ('36000000-0000-0000-0000-000000000021', N'COOL-ARCTIC-LF-III-360', N'MAG-01 Centrum Komponentow', N'B-PSU', 10, 0),
        ('36000000-0000-0000-0000-000000000022', N'NET-TP-SG108', N'MAG-01 Centrum Komponentow', N'C-NET', 31, 0),
        ('36000000-0000-0000-0000-000000000023', N'NET-UBI-U6-LITE', N'MAG-01 Centrum Komponentow', N'C-NET', 14, 0),
        ('36000000-0000-0000-0000-000000000024', N'KB-LOGI-MX-KEYS', N'MAG-01 Centrum Komponentow', N'C-NET', 19, 0),
        ('36000000-0000-0000-0000-000000000025', N'MOUSE-LOGI-MX-MASTER-3S', N'MAG-01 Centrum Komponentow', N'C-NET', 20, 0),
        ('36000000-0000-0000-0000-000000000026', N'MON-DELL-U2723QE', N'MAG-01 Centrum Komponentow', N'PICK-01', 8, 0),
        ('36000000-0000-0000-0000-000000000027', N'MON-LG-27GP850', N'MAG-01 Centrum Komponentow', N'PICK-01', 10, 0),
        ('36000000-0000-0000-0000-000000000028', N'ACC-PASTE-MX6-4G', N'MAG-01 Centrum Komponentow', N'PICK-01', 95, 0),
        ('36000000-0000-0000-0000-000000000029', N'ACC-CABLE-DP14-2M', N'MAG-01 Centrum Komponentow', N'PICK-01', 88, 0),
        ('36000000-0000-0000-0000-000000000030', N'TOOL-ESD-MAT', N'MAG-02 Serwis i Zwroty', N'QC-01', 25, 0);

    ;WITH InventorySource AS
    (
        SELECT
            products.ProductID,
            inventory.InventoryID,
            locations.LocationID,
            inventory.Quantity,
            inventory.ReservedQuantity
        FROM @Inventory AS inventory
        INNER JOIN dbo.Products AS products
            ON products.SKU = inventory.SKU
        INNER JOIN dbo.Warehouses AS warehouses
            ON warehouses.Name = inventory.WarehouseName
        INNER JOIN dbo.Locations AS locations
            ON locations.WarehouseID = warehouses.WarehouseID
            AND locations.LocationCode = inventory.LocationCode
    )
    MERGE dbo.Inventory AS target
    USING InventorySource AS source
        ON target.ProductID = source.ProductID
        AND target.LocationID = source.LocationID
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (InventoryID, ProductID, LocationID, Quantity, ReservedQuantity)
        VALUES (source.InventoryID, source.ProductID, source.LocationID, source.Quantity, source.ReservedQuantity);

    COMMIT TRANSACTION;

    SELECT N'Production computer-parts seed applied.' AS Message;

    SELECT N'Products' AS Entity, COUNT(*) AS SeededRows
    FROM dbo.Products
    WHERE SKU IN (SELECT SKU FROM @Products)
    UNION ALL
    SELECT N'Inventory entries' AS Entity, COUNT(*) AS SeededRows
    FROM dbo.Inventory AS inventory
    INNER JOIN dbo.Products AS products
        ON products.ProductID = inventory.ProductID
    WHERE products.SKU IN (SELECT SKU FROM @Products)
    UNION ALL
    SELECT N'Warehouses' AS Entity, COUNT(*) AS SeededRows
    FROM dbo.Warehouses
    WHERE Name IN (SELECT Name FROM @Warehouses);
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
