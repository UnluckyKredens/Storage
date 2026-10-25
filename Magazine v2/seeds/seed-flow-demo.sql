/*
    Przykładowy przepływ towaru w sieci czterech magazynów (dane operacyjne do prezentacji).

    Fabuła (ostatnie ~14 dni, daty liczone od momentu uruchomienia skryptu):
      1. Gdańsk przyjmuje zamówienie zewnętrzne (kontener z Azji) - 8 pozycji.
      2. Gdańsk rozsyła towar wysyłkami: Kraków 55%, Warszawa 20%, Poznań 15% (około 10% zostaje w Gdańsku).
      3. Kraków uzupełnia Warszawę (SSD) i Poznań (RAM) kolejnymi wysyłkami.
      4. Kraków wydaje towar wewnętrznie (RW) i przenosi część RAM-u z REC-01 na regał A-01 (MM).

    Skrypt zapisuje spójnie: magazyny i lokalizacje (REC-01, A-01, PICK-01), kontrahenta, zamówienie ZZ, wysyłki WZ,
    dokumenty RW/MM, stany, księgę ruchów i liczniki numeracji. Zdarzenia historii (strumień na dashboard kierownika)
    trafiają do tabeli outbox, a uruchomione API przenosi je do bazy historii w ciągu kilku sekund.

    Wymagania:
      - baza po migracjach EF Core i załadowany katalog: seeds/seed-computer-parts-items.sql,
      - istnieje konto administratora (login "admin"), które staje się autorem dokumentów.

    Magazyny dopasowywane są po nazwie (poniżej). Zmień nazwy, jeśli na instancji nazywają się inaczej;
    brakujące magazyny zostaną utworzone.

    Idempotencja: zamówienie z notatką FLOW-DEMO oznacza, że przepływ już załadowano - skrypt kończy się bez zmian.
    Skrypt wymaga magazynów bez stanów tych produktów (przerywa z błędem, gdy stany już istnieją).

    Użycie (flaga -I jest wymagana):
      sqlcmd -C -I -f 65001 -S <serwer> -d <baza> -U <użytkownik> -P <hasło> -i seeds/seed-flow-demo.sql
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @GdanskName nvarchar(200) = N'Przeładunkowy Gdańsk';
DECLARE @KrakowName nvarchar(200) = N'Główny Kraków';
DECLARE @WarszawaName nvarchar(200) = N'Warszawa – Oddział';
DECLARE @PoznanName nvarchar(200) = N'Poznań – Oddział';

IF EXISTS (SELECT 1 FROM dbo.PurchaseOrders WHERE Notes = N'FLOW-DEMO')
BEGIN
    PRINT N'Przepływ demonstracyjny jest już załadowany - nic nie zrobiono.';
    RETURN;
END;

DECLARE @Admin uniqueidentifier =
(
    SELECT TOP (1) u.Id
    FROM dbo.Users u
    JOIN dbo.Roles r ON r.Id = u.RoleId
    WHERE r.Name = N'Administrator'
    ORDER BY CASE WHEN u.Login = N'admin' THEN 0 ELSE 1 END, u.Login
);

IF @Admin IS NULL
    THROW 50001, N'Brak konta administratora. Uruchom API, aby je utworzyć, i spróbuj ponownie.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Now datetime2 = SYSUTCDATETIME();

    ---------------------------------------------------------------------------
    -- Magazyny i lokalizacje
    ---------------------------------------------------------------------------
    INSERT dbo.Warehouses (Id, Name, Address, Description)
    SELECT NEWID(), w.Name, w.Address, w.Description
    FROM (VALUES
        (@GdanskName,   N'Gdańsk, Port',     N'Magazyn przeładunkowy przy porcie'),
        (@KrakowName,   N'Kraków',           N'Magazyn główny'),
        (@WarszawaName, N'Warszawa',         N'Oddział regionalny'),
        (@PoznanName,   N'Poznań',           N'Oddział regionalny')) AS w (Name, Address, Description)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Warehouses x WHERE x.Name = w.Name);

    INSERT dbo.Locations (Id, WarehouseId, LocationCode, Description)
    SELECT NEWID(), w.Id, l.Code, l.Description
    FROM dbo.Warehouses w
    CROSS JOIN (VALUES (N'REC-01', N'Przyjęcia'), (N'A-01', N'Regał A'), (N'PICK-01', N'Kompletacja')) AS l (Code, Description)
    WHERE w.Name IN (@GdanskName, @KrakowName, @WarszawaName, @PoznanName)
      AND NOT EXISTS (SELECT 1 FROM dbo.Locations x WHERE x.WarehouseId = w.Id AND x.LocationCode = l.Code);

    DECLARE @Gda uniqueidentifier = (SELECT Id FROM dbo.Warehouses WHERE Name = @GdanskName);
    DECLARE @Krk uniqueidentifier = (SELECT Id FROM dbo.Warehouses WHERE Name = @KrakowName);
    DECLARE @Waw uniqueidentifier = (SELECT Id FROM dbo.Warehouses WHERE Name = @WarszawaName);
    DECLARE @Poz uniqueidentifier = (SELECT Id FROM dbo.Warehouses WHERE Name = @PoznanName);

    IF NOT EXISTS (SELECT 1 FROM dbo.Contractors WHERE Name = N'Shenzhen Components Ltd.')
        INSERT dbo.Contractors (Id, Name, TaxNumber, Type, Email, Phone, Address)
        VALUES (NEWID(), N'Shenzhen Components Ltd.', N'9876543210', N'Supplier',
                N'sales@szcomponents.example', N'+86 755 0000 0000', N'Shenzhen, Chiny');

    DECLARE @Supplier uniqueidentifier = (SELECT Id FROM dbo.Contractors WHERE Name = N'Shenzhen Components Ltd.');

    ---------------------------------------------------------------------------
    -- Zamówienie zewnętrzne do Gdańska (ilości z kontenera)
    ---------------------------------------------------------------------------
    CREATE TABLE #Po (ProductId uniqueidentifier, Qty decimal(18,3));

    INSERT #Po (ProductId, Qty)
    SELECT p.Id, v.Qty
    FROM (VALUES
        (N'CZ-CPU-R5-7600', 200), (N'CZ-CPU-I5-14400F', 200), (N'CZ-GPU-RTX4060', 150), (N'CZ-GPU-RX7600', 100),
        (N'CZ-MB-B650', 100), (N'CZ-RAM-D5-16', 600), (N'CZ-SSD-1T', 400), (N'CZ-PSU-650G', 150)) AS v (Sku, Qty)
    JOIN dbo.Products p ON p.SKU = v.Sku;

    IF (SELECT COUNT(*) FROM #Po) < 8
        THROW 50002, N'Brak części produktów. Najpierw załaduj seeds/seed-computer-parts-items.sql.', 1;

    ---------------------------------------------------------------------------
    -- Dokumenty: PO (ZZ), wysyłki (WZ), wydania wewnętrzne (RW), przesunięcia (MM)
    ---------------------------------------------------------------------------
    CREATE TABLE #Doc
    (
        DocId uniqueidentifier NOT NULL PRIMARY KEY,
        Kind varchar(10) NOT NULL,       -- PO | SHIP | ISSUE | MOVE
        Code char(2) NOT NULL,           -- ZZ | WZ | RW | MM
        OwnerWh uniqueidentifier NOT NULL,
        DstWh uniqueidentifier NULL,
        At datetime2 NOT NULL,
        RecvAt datetime2 NULL,
        Num int NULL,
        Number nvarchar(50) NULL
    );
    CREATE TABLE #Line (DocId uniqueidentifier NOT NULL, ProductId uniqueidentifier NOT NULL, Qty decimal(18,3) NOT NULL);

    DECLARE @PoId uniqueidentifier = NEWID();
    DECLARE @PoAt datetime2 = DATEADD(HOUR, 9, CAST(CAST(DATEADD(DAY, -14, @Now) AS date) AS datetime2));
    INSERT #Doc (DocId, Kind, Code, OwnerWh, At, RecvAt) VALUES (@PoId, 'PO', 'ZZ', @Gda, @PoAt, DATEADD(DAY, 1, @PoAt));
    INSERT #Line SELECT @PoId, ProductId, Qty FROM #Po;

    -- Rozdział z Gdańska
    DECLARE @S1 uniqueidentifier = NEWID(), @S2 uniqueidentifier = NEWID(), @S3 uniqueidentifier = NEWID();
    DECLARE @ShipBase datetime2 = DATEADD(HOUR, 10, CAST(CAST(DATEADD(DAY, -12, @Now) AS date) AS datetime2));
    INSERT #Doc (DocId, Kind, Code, OwnerWh, DstWh, At, RecvAt) VALUES
        (@S1, 'SHIP', 'WZ', @Gda, @Krk, @ShipBase,                      DATEADD(DAY, 1, @ShipBase)),
        (@S2, 'SHIP', 'WZ', @Gda, @Waw, DATEADD(MINUTE, 20, @ShipBase), DATEADD(DAY, 2, @ShipBase)),
        (@S3, 'SHIP', 'WZ', @Gda, @Poz, DATEADD(MINUTE, 40, @ShipBase), DATEADD(DAY, 3, @ShipBase));
    INSERT #Line SELECT @S1, ProductId, FLOOR(Qty * 0.55) FROM #Po;
    INSERT #Line SELECT @S2, ProductId, FLOOR(Qty * 0.20) FROM #Po;
    INSERT #Line SELECT @S3, ProductId, FLOOR(Qty * 0.15) FROM #Po;

    -- Uzupełnienia z Krakowa
    DECLARE @S4 uniqueidentifier = NEWID(), @S5 uniqueidentifier = NEWID();
    DECLARE @T4 datetime2 = DATEADD(HOUR, 11, CAST(CAST(DATEADD(DAY, -6, @Now) AS date) AS datetime2));
    DECLARE @T5 datetime2 = DATEADD(HOUR, 11, CAST(CAST(DATEADD(DAY, -5, @Now) AS date) AS datetime2));
    INSERT #Doc (DocId, Kind, Code, OwnerWh, DstWh, At, RecvAt) VALUES
        (@S4, 'SHIP', 'WZ', @Krk, @Waw, @T4, DATEADD(DAY, 1, @T4)),
        (@S5, 'SHIP', 'WZ', @Krk, @Poz, @T5, DATEADD(DAY, 1, @T5));
    INSERT #Line SELECT @S4, Id, 50 FROM dbo.Products WHERE SKU = N'CZ-SSD-1T';
    INSERT #Line SELECT @S5, Id, 100 FROM dbo.Products WHERE SKU = N'CZ-RAM-D5-16';

    -- Wydanie wewnętrzne i przesunięcie w Krakowie
    DECLARE @I1 uniqueidentifier = NEWID(), @M1 uniqueidentifier = NEWID();
    DECLARE @T6 datetime2 = DATEADD(HOUR, 13, CAST(CAST(DATEADD(DAY, -3, @Now) AS date) AS datetime2));
    DECLARE @T7 datetime2 = DATEADD(HOUR, 9,  CAST(CAST(DATEADD(DAY, -2, @Now) AS date) AS datetime2));
    INSERT #Doc (DocId, Kind, Code, OwnerWh, At) VALUES (@I1, 'ISSUE', 'RW', @Krk, @T6), (@M1, 'MOVE', 'MM', @Krk, @T7);
    INSERT #Line SELECT @I1, Id, 20 FROM dbo.Products WHERE SKU = N'CZ-RAM-D5-16';
    INSERT #Line SELECT @I1, Id, 10 FROM dbo.Products WHERE SKU = N'CZ-SSD-1T';
    INSERT #Line SELECT @M1, Id, 60 FROM dbo.Products WHERE SKU = N'CZ-RAM-D5-16';

    ---------------------------------------------------------------------------
    -- Numeracja dokumentów w formacie aplikacji: <ODDZIAŁ>/<typ>/<rok>/<000001>, np. WARSZAWA/ZZ/2026/000001
    ---------------------------------------------------------------------------
    ;WITH ranked AS
    (
        SELECT d.DocId, d.OwnerWh, d.Code, YEAR(d.At) AS Yr,
               ROW_NUMBER() OVER (PARTITION BY d.OwnerWh, d.Code, YEAR(d.At) ORDER BY d.At, d.DocId) AS Rn
        FROM #Doc d
    )
    UPDATE d
    SET Num = r.Rn + ISNULL(s.LastNumber, 0),
        Number = UPPER(TRANSLATE(LEFT(REPLACE(h.Head, N' ', N''), 16), N'ąćęłńóśźżĄĆĘŁŃÓŚŹŻ', N'acelnoszzACELNOSZZ')) + N'/' + d.Code + N'/'
                 + CAST(r.Yr AS nvarchar(4)) + N'/' + RIGHT(N'000000' + CAST(r.Rn + ISNULL(s.LastNumber, 0) AS nvarchar(10)), 6)
    FROM #Doc d
    JOIN ranked r ON r.DocId = d.DocId
    JOIN dbo.Warehouses w ON w.Id = d.OwnerWh
    CROSS APPLY (SELECT CASE WHEN CHARINDEX(N' – ', w.Name) > 0 THEN LEFT(w.Name, CHARINDEX(N' – ', w.Name) - 1) ELSE w.Name END AS Head) h
    LEFT JOIN dbo.DocumentNumberSequences s
        ON s.WarehouseId = d.OwnerWh AND s.DocumentType = d.Code AND s.Year = r.Yr;

    MERGE dbo.DocumentNumberSequences AS t
    USING (SELECT OwnerWh, Code, YEAR(At) AS Yr, MAX(Num) AS LastNum FROM #Doc GROUP BY OwnerWh, Code, YEAR(At)) AS s
        ON t.WarehouseId = s.OwnerWh AND t.DocumentType = s.Code AND t.Year = s.Yr
    WHEN MATCHED THEN UPDATE SET LastNumber = s.LastNum, UpdatedOnUtc = @Now
    WHEN NOT MATCHED THEN INSERT (Id, WarehouseId, DocumentType, Year, LastNumber, UpdatedOnUtc)
        VALUES (NEWID(), s.OwnerWh, s.Code, s.Yr, s.LastNum, @Now);

    ---------------------------------------------------------------------------
    -- Rekordy dokumentów
    ---------------------------------------------------------------------------
    INSERT dbo.PurchaseOrders (Id, Number, WarehouseId, ContractorId, CreatedByUserId, CreatedOnUtc, ApprovedByUserId, ApprovedOnUtc,
                               ReceivedByUserId, ReceivedOnUtc, InvoiceNumber, PaperDocumentNumber, Notes, Status)
    SELECT d.DocId, d.Number, d.OwnerWh, @Supplier, @Admin, d.At, @Admin, DATEADD(HOUR, 1, d.At),
           @Admin, d.RecvAt, N'FV/SZ/2026/0042', NULL, N'FLOW-DEMO', 3
    FROM #Doc d WHERE d.Kind = 'PO';

    INSERT dbo.PurchaseOrderItems (Id, PurchaseOrderId, ProductId, Barcode, Quantity, UnitPrice)
    SELECT NEWID(), l.DocId, l.ProductId, ISNULL(p.Barcode, N''), l.Qty, p.PurchasePrice
    FROM #Line l
    JOIN #Doc d ON d.DocId = l.DocId AND d.Kind = 'PO'
    JOIN dbo.Products p ON p.Id = l.ProductId;

    INSERT dbo.Shipments (Id, Number, SourceWarehouseId, DestinationWarehouseId, CreatedByUserId, CreatedOnUtc,
                          ApprovedByUserId, ApprovedOnUtc, ReceivedByUserId, ReceivedOnUtc, Status)
    SELECT d.DocId, d.Number, d.OwnerWh, d.DstWh, @Admin, d.At, @Admin, DATEADD(HOUR, 1, d.At), @Admin, d.RecvAt, 4
    FROM #Doc d WHERE d.Kind = 'SHIP';

    INSERT dbo.ShipmentItems (Id, ShipmentId, ProductId, Barcode, Quantity)
    SELECT NEWID(), l.DocId, l.ProductId, ISNULL(p.Barcode, N''), l.Qty
    FROM #Line l
    JOIN #Doc d ON d.DocId = l.DocId AND d.Kind = 'SHIP'
    JOIN dbo.Products p ON p.Id = l.ProductId;

    INSERT dbo.WarehouseOperations (Id, Number, Type, Status, WarehouseId, CreatedByUserId, CreatedOnUtc, CompletedByUserId, CompletedOnUtc, Notes)
    SELECT d.DocId, d.Number, CASE d.Kind WHEN 'ISSUE' THEN 2 ELSE 3 END, 1, d.OwnerWh, @Admin, d.At, @Admin, d.At,
           CASE d.Kind WHEN 'ISSUE' THEN N'FLOW-DEMO: wydanie na stanowisko serwisowe' ELSE N'FLOW-DEMO: przesunięcie na regał A-01' END
    FROM #Doc d WHERE d.Kind IN ('ISSUE', 'MOVE');

    INSERT dbo.WarehouseOperationItems (Id, WarehouseOperationId, ProductId, SourceLocationId, DestinationLocationId, Quantity, TargetQuantity)
    SELECT NEWID(), l.DocId, l.ProductId, rec.Id, CASE d.Kind WHEN 'MOVE' THEN a.Id END, l.Qty, NULL
    FROM #Line l
    JOIN #Doc d ON d.DocId = l.DocId AND d.Kind IN ('ISSUE', 'MOVE')
    JOIN dbo.Locations rec ON rec.WarehouseId = d.OwnerWh AND rec.LocationCode = N'REC-01'
    JOIN dbo.Locations a ON a.WarehouseId = d.OwnerWh AND a.LocationCode = N'A-01';

    ---------------------------------------------------------------------------
    -- Zdarzenia magazynowe -> stany, księga ruchów, strumień historii
    ---------------------------------------------------------------------------
    CREATE TABLE #Ev
    (
        Ord int IDENTITY(1,1) PRIMARY KEY,
        At datetime2 NOT NULL, WarehouseId uniqueidentifier NOT NULL, LocationId uniqueidentifier NOT NULL,
        ProductId uniqueidentifier NOT NULL, Change decimal(18,3) NOT NULL, Type int NOT NULL,
        SourceType nvarchar(50) NOT NULL, SourceId uniqueidentifier NOT NULL, SourceNumber nvarchar(50) NOT NULL, Notes nvarchar(200) NULL
    );

    ;WITH loc AS (SELECT WarehouseId, MAX(CASE WHEN LocationCode = N'REC-01' THEN Id END) AS Rec, MAX(CASE WHEN LocationCode = N'A-01' THEN Id END) AS A
                  FROM dbo.Locations GROUP BY WarehouseId),
    ev AS
    (
        SELECT d.RecvAt AS At, d.OwnerWh AS WarehouseId, lo.Rec AS LocationId, l.ProductId, l.Qty AS Change, 12 AS Type,
               N'PurchaseOrder' AS SourceType, d.DocId AS SourceId, d.Number AS SourceNumber, N'Przyjęcie dostawy z zamówienia zewnętrznego' AS Notes
        FROM #Doc d JOIN #Line l ON l.DocId = d.DocId JOIN loc lo ON lo.WarehouseId = d.OwnerWh WHERE d.Kind = 'PO'
        UNION ALL
        SELECT DATEADD(HOUR, 1, d.At), d.OwnerWh, lo.Rec, l.ProductId, -l.Qty, 2, N'Shipment', d.DocId, d.Number, N'Wysyłka do innego magazynu'
        FROM #Doc d JOIN #Line l ON l.DocId = d.DocId JOIN loc lo ON lo.WarehouseId = d.OwnerWh WHERE d.Kind = 'SHIP'
        UNION ALL
        SELECT d.RecvAt, d.DstWh, lo.Rec, l.ProductId, l.Qty, 3, N'Shipment', d.DocId, d.Number, N'Przyjęcie wysyłki'
        FROM #Doc d JOIN #Line l ON l.DocId = d.DocId JOIN loc lo ON lo.WarehouseId = d.DstWh WHERE d.Kind = 'SHIP'
        UNION ALL
        SELECT d.At, d.OwnerWh, lo.Rec, l.ProductId, -l.Qty, 9, N'WarehouseOperation', d.DocId, d.Number, N'Wydanie wewnętrzne'
        FROM #Doc d JOIN #Line l ON l.DocId = d.DocId JOIN loc lo ON lo.WarehouseId = d.OwnerWh WHERE d.Kind = 'ISSUE'
        UNION ALL
        SELECT d.At, d.OwnerWh, lo.Rec, l.ProductId, -l.Qty, 10, N'WarehouseOperation', d.DocId, d.Number, N'Przesunięcie: wyjście z REC-01'
        FROM #Doc d JOIN #Line l ON l.DocId = d.DocId JOIN loc lo ON lo.WarehouseId = d.OwnerWh WHERE d.Kind = 'MOVE'
        UNION ALL
        SELECT d.At, d.OwnerWh, lo.A, l.ProductId, l.Qty, 11, N'WarehouseOperation', d.DocId, d.Number, N'Przesunięcie: wejście na A-01'
        FROM #Doc d JOIN #Line l ON l.DocId = d.DocId JOIN loc lo ON lo.WarehouseId = d.OwnerWh WHERE d.Kind = 'MOVE'
    )
    INSERT #Ev (At, WarehouseId, LocationId, ProductId, Change, Type, SourceType, SourceId, SourceNumber, Notes)
    SELECT At, WarehouseId, LocationId, ProductId, Change, Type, SourceType, SourceId, SourceNumber, Notes
    FROM ev ORDER BY At, SourceNumber, Type, ProductId;

    -- Stany końcowe = suma zmian. Dokumenty są zaplanowane tak, aby stan nigdy nie spadł poniżej zera.
    CREATE TABLE #Run
    (
        Ord int PRIMARY KEY, Before decimal(18,3), After decimal(18,3)
    );
    INSERT #Run (Ord, Before, After)
    SELECT Ord,
           SUM(Change) OVER (PARTITION BY LocationId, ProductId ORDER BY Ord ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING),
           SUM(Change) OVER (PARTITION BY LocationId, ProductId ORDER BY Ord)
    FROM #Ev;

    IF EXISTS (SELECT 1 FROM #Run WHERE After < 0)
        THROW 50003, N'Przepływ demonstracyjny doprowadziłby do ujemnego stanu - przerwano.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Inventory i JOIN #Ev e ON e.ProductId = i.ProductId AND e.LocationId = i.LocationId)
        THROW 50004, N'W tych magazynach są już stany tych produktów - przepływ demonstracyjny wymaga pustych magazynów.', 1;

    INSERT dbo.Inventory (Id, ProductId, LocationId, Quantity, ReservedQuantity)
    SELECT NEWID(), e.ProductId, e.LocationId, SUM(e.Change), 0
    FROM #Ev e
    GROUP BY e.ProductId, e.LocationId;

    INSERT dbo.StockMovements (Id, WarehouseId, LocationId, ProductId, InventoryId, Type, QuantityBefore, QuantityChange, QuantityAfter,
                               ReservedQuantityBefore, ReservedQuantityChange, ReservedQuantityAfter,
                               SourceType, SourceId, SourceNumber, CreatedByUserId, CreatedOnUtc, Notes)
    SELECT NEWID(), e.WarehouseId, e.LocationId, e.ProductId, i.Id, e.Type, ISNULL(r.Before, 0), e.Change, r.After,
           0, 0, 0, e.SourceType, e.SourceId, e.SourceNumber, @Admin, e.At, e.Notes
    FROM #Ev e
    JOIN #Run r ON r.Ord = e.Ord
    JOIN dbo.Inventory i ON i.ProductId = e.ProductId AND i.LocationId = e.LocationId;

    ---------------------------------------------------------------------------
    -- Outbox historii (przetwarza go API): zmiany stanów i zdarzenia wysyłek
    ---------------------------------------------------------------------------
    INSERT dbo.HistoryOutboxMessages (Id, EntityType, Payload, CreatedOnUtc, ProcessedOnUtc)
    SELECT h.Id, N'InventoryHistory',
           (SELECT h.Id AS Id, i.Id AS ItemId, e.ProductId AS ProductId, e.LocationId AS LocationId, r.After AS Quantity,
                   CAST(0 AS decimal(18,3)) AS ReservedQuantity, e.SourceId AS AggregateId, e.Change AS QuantityChange,
                   CONVERT(varchar(33), e.At, 126) AS CreatedOn
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           e.At, NULL
    FROM #Ev e
    JOIN #Run r ON r.Ord = e.Ord
    JOIN dbo.Inventory i ON i.ProductId = e.ProductId AND i.LocationId = e.LocationId
    CROSS APPLY (SELECT NEWID() AS Id) h;

    INSERT dbo.HistoryOutboxMessages (Id, EntityType, Payload, CreatedOnUtc, ProcessedOnUtc)
    SELECT h.Id, N'ShipmentHistory',
           (SELECT h.Id AS Id, d.DocId AS ShipmentId, ev.EventType AS EventType, d.OwnerWh AS SourceWarehouseId,
                   d.DstWh AS DestinationWarehouseId, @Admin AS UserId, CONVERT(varchar(33), ev.EventAt, 126) AS CreatedOnUtc,
                   ev.Details AS Details
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           ev.EventAt, NULL
    FROM #Doc d
    CROSS APPLY (VALUES
        (N'created',    d.At,                      N'Utworzono wysyłkę ' + d.Number),
        (N'approved',   DATEADD(HOUR, 1, d.At),    N'Zatwierdzono wysyłkę ' + d.Number),
        (N'in_transit', DATEADD(HOUR, 1, d.At),    N'Towar w drodze'),
        (N'received',   d.RecvAt,                  N'Przyjęto wysyłkę ' + d.Number)) AS ev (EventType, EventAt, Details)
    CROSS APPLY (SELECT NEWID() AS Id) h
    WHERE d.Kind = 'SHIP';

    COMMIT TRANSACTION;

    SELECT N'Dokumenty' AS Podsumowanie, d.Code AS Typ, COUNT(*) AS Liczba FROM #Doc d GROUP BY d.Code
    UNION ALL SELECT N'Ruchy magazynowe', N'', COUNT(*) FROM #Ev
    UNION ALL SELECT N'Pozycje stanów', N'', COUNT(DISTINCT CONCAT(ProductId, LocationId)) FROM #Ev;

    DROP TABLE #Run, #Ev, #Line, #Doc, #Po;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
