/*
    Katalog części komputerowych: 87 przedmiotów w 13 kategoriach.

    Skrypt dodaje WYŁĄCZNIE kategorie, jednostki miary i produkty (z cenami, progami minimum/optimum
    i kodami EAN-13). Nie tworzy magazynów, lokalizacji, stanów, dokumentów ani użytkowników,
    więc wszystkie dane operacyjne powstają w scenariuszu (seeds/scenariusz-czesci-komputerowe.md).

    Zawiera dwie pozycje wycofane (IsActive = 0): CZ-CPU-I7-12700K i CZ-GPU-GTX1660S.
    SKU mają prefiks CZ-, więc nie kolidują z innymi skryptami seedującymi.

    Idempotencja: jednostki dopasowywane po symbolu, kategorie po nazwie, produkty po SKU.
    Ponowne uruchomienie aktualizuje opisy, ceny i progi i nie dubluje danych.

    Użycie (SQL Server, baza po migracjach EF Core; flaga -I jest wymagana):
      sqlcmd -C -I -f 65001 -S <serwer> -d <baza> -U <użytkownik> -P <hasło> -i seeds/seed-computer-parts-items.sql
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    ---------------------------------------------------------------------------
    -- Jednostki miary
    ---------------------------------------------------------------------------
    MERGE dbo.UnitsOfMeasure AS target
    USING
    (
        VALUES
            (N'Sztuka', N'szt.'),
            (N'Komplet', N'kpl.'),
            (N'Opakowanie', N'op.'),
            (N'Metr', N'm')
    ) AS source (Name, Symbol)
        ON target.Symbol = source.Symbol
    WHEN NOT MATCHED THEN
        INSERT (Id, Name, Symbol)
        VALUES (NEWID(), source.Name, source.Symbol);

    ---------------------------------------------------------------------------
    -- Kategorie
    ---------------------------------------------------------------------------
    MERGE dbo.Categories AS target
    USING
    (
        VALUES
            (N'Procesory', N'Procesory AMD i Intel do komputerów stacjonarnych.'),
            (N'Karty graficzne', N'Karty graficzne NVIDIA i AMD.'),
            (N'Płyty główne', N'Płyty główne pod platformy AM4, AM5 i LGA1700.'),
            (N'Pamięci RAM', N'Moduły i zestawy pamięci DDR4 oraz DDR5.'),
            (N'Dyski SSD i HDD', N'Dyski NVMe, SATA SSD oraz dyski talerzowe.'),
            (N'Zasilacze', N'Zasilacze ATX od 550 W do 1000 W.'),
            (N'Obudowy', N'Obudowy komputerowe od Mini-ITX do Full Tower.'),
            (N'Chłodzenie', N'Chłodzenie procesorów, wentylatory i pasty.'),
            (N'Monitory', N'Monitory biurowe i dla graczy.'),
            (N'Klawiatury i myszy', N'Klawiatury, myszy i zestawy.'),
            (N'Sieć i karty rozszerzeń', N'Karty sieciowe, kontrolery i urządzenia sieciowe.'),
            (N'Kable i akcesoria', N'Kable, adaptery i drobne akcesoria montażowe.'),
            (N'Komputery i laptopy', N'Gotowe zestawy komputerowe i laptopy.')
    ) AS source (Name, Description)
        ON target.Name = source.Name
    WHEN MATCHED THEN
        UPDATE SET Description = source.Description
    WHEN NOT MATCHED THEN
        INSERT (Id, Name, Description)
        VALUES (NEWID(), source.Name, source.Description);

    ---------------------------------------------------------------------------
    -- Produkty
    -- Kolumny źródła: SKU, Name, Barcode, Description, UnitSymbol, CategoryName,
    --                 PurchasePrice, SalePrice, MinimumQuantity, OptimumQuantity, IsActive
    ---------------------------------------------------------------------------
    MERGE dbo.Products AS target
    USING
    (
        SELECT
            seed.SKU,
            seed.Name,
            seed.Barcode,
            seed.Description,
            unitOfMeasure.Id AS UnitOfMeasureId,
            category.Id AS CategoryId,
            seed.PurchasePrice,
            seed.SalePrice,
            seed.MinimumQuantity,
            seed.OptimumQuantity,
            seed.IsActive
        FROM
        (
            VALUES
            (N'CZ-CPU-R5-5600', N'Procesor AMD Ryzen 5 5600 AM4', N'5909002000016', N'6 rdzeni, 3,5 GHz, BOX z chłodzeniem.', N'szt.', N'Procesory', 390.00, 549.00, 15, 60, 1),
            (N'CZ-CPU-R5-7600', N'Procesor AMD Ryzen 5 7600 AM5', N'5909002000023', N'6 rdzeni, 3,8 GHz, grafika zintegrowana.', N'szt.', N'Procesory', 640.00, 849.00, 20, 80, 1),
            (N'CZ-CPU-R7-7800X3D', N'Procesor AMD Ryzen 7 7800X3D AM5', N'5909002000030', N'8 rdzeni z pamięcią 3D V-Cache.', N'szt.', N'Procesory', 1380.00, 1749.00, 10, 40, 1),
            (N'CZ-CPU-R9-7950X', N'Procesor AMD Ryzen 9 7950X AM5', N'5909002000047', N'16 rdzeni, 4,5 GHz.', N'szt.', N'Procesory', 1950.00, 2449.00, 5, 20, 1),
            (N'CZ-CPU-I5-14400F', N'Procesor Intel Core i5-14400F LGA1700', N'5909002000054', N'10 rdzeni, bez grafiki zintegrowanej.', N'szt.', N'Procesory', 720.00, 949.00, 20, 80, 1),
            (N'CZ-CPU-I5-14600K', N'Procesor Intel Core i5-14600K LGA1700', N'5909002000061', N'14 rdzeni, odblokowany mnożnik.', N'szt.', N'Procesory', 1020.00, 1349.00, 10, 40, 1),
            (N'CZ-CPU-I7-14700K', N'Procesor Intel Core i7-14700K LGA1700', N'5909002000078', N'20 rdzeni, odblokowany mnożnik.', N'szt.', N'Procesory', 1480.00, 1899.00, 8, 32, 1),
            (N'CZ-CPU-I9-14900K', N'Procesor Intel Core i9-14900K LGA1700', N'5909002000085', N'24 rdzenie, do 6,0 GHz.', N'szt.', N'Procesory', 2150.00, 2749.00, 4, 16, 1),
            (N'CZ-CPU-I7-12700K', N'Procesor Intel Core i7-12700K LGA1700 (wycofany)', N'5909002000092', N'Model wycofany. Pozycja nieaktywna do testu filtrowania.', N'szt.', N'Procesory', 900.00, 1199.00, 0, 0, 0),
            (N'CZ-GPU-RTX4060', N'Karta graficzna GeForce RTX 4060 8 GB', N'5909002000108', N'8 GB GDDR6, DLSS 3.', N'szt.', N'Karty graficzne', 1150.00, 1549.00, 10, 40, 1),
            (N'CZ-GPU-RTX4060TI', N'Karta graficzna GeForce RTX 4060 Ti 8 GB', N'5909002000115', N'8 GB GDDR6, DLSS 3.', N'szt.', N'Karty graficzne', 1620.00, 2099.00, 8, 32, 1),
            (N'CZ-GPU-RTX4070S', N'Karta graficzna GeForce RTX 4070 SUPER 12 GB', N'5909002000122', N'12 GB GDDR6X.', N'szt.', N'Karty graficzne', 2450.00, 3099.00, 6, 24, 1),
            (N'CZ-GPU-RTX4080S', N'Karta graficzna GeForce RTX 4080 SUPER 16 GB', N'5909002000139', N'16 GB GDDR6X.', N'szt.', N'Karty graficzne', 4300.00, 5399.00, 3, 12, 1),
            (N'CZ-GPU-RTX4090', N'Karta graficzna GeForce RTX 4090 24 GB', N'5909002000146', N'24 GB GDDR6X, 450 W.', N'szt.', N'Karty graficzne', 7300.00, 8999.00, 2, 6, 1),
            (N'CZ-GPU-RX7600', N'Karta graficzna Radeon RX 7600 8 GB', N'5909002000153', N'8 GB GDDR6.', N'szt.', N'Karty graficzne', 920.00, 1249.00, 8, 32, 1),
            (N'CZ-GPU-RX7800XT', N'Karta graficzna Radeon RX 7800 XT 16 GB', N'5909002000160', N'16 GB GDDR6.', N'szt.', N'Karty graficzne', 1980.00, 2549.00, 5, 20, 1),
            (N'CZ-GPU-RX7900XTX', N'Karta graficzna Radeon RX 7900 XTX 24 GB', N'5909002000177', N'24 GB GDDR6.', N'szt.', N'Karty graficzne', 3900.00, 4949.00, 3, 10, 1),
            (N'CZ-GPU-GTX1660S', N'Karta graficzna GeForce GTX 1660 SUPER 6 GB (wycofana)', N'5909002000184', N'Model wycofany. Pozycja nieaktywna.', N'szt.', N'Karty graficzne', 650.00, 899.00, 0, 0, 0),
            (N'CZ-MB-A620M', N'Płyta główna A620M mATX AM5', N'5909002000191', N'DDR5, M.2, Wi-Fi opcjonalne.', N'szt.', N'Płyty główne', 390.00, 549.00, 10, 40, 1),
            (N'CZ-MB-B650', N'Płyta główna B650 ATX AM5', N'5909002000207', N'DDR5, PCIe 5.0 M.2.', N'szt.', N'Płyty główne', 690.00, 949.00, 12, 48, 1),
            (N'CZ-MB-X670E', N'Płyta główna X670E ATX AM5', N'5909002000214', N'DDR5, PCIe 5.0, Wi-Fi 6E.', N'szt.', N'Płyty główne', 1350.00, 1749.00, 4, 16, 1),
            (N'CZ-MB-B550', N'Płyta główna B550 ATX AM4', N'5909002000221', N'DDR4, PCIe 4.0.', N'szt.', N'Płyty główne', 420.00, 599.00, 8, 32, 1),
            (N'CZ-MB-H610M', N'Płyta główna H610M mATX LGA1700', N'5909002000238', N'DDR4, do biurowych zestawów.', N'szt.', N'Płyty główne', 310.00, 449.00, 10, 40, 1),
            (N'CZ-MB-B760', N'Płyta główna B760 ATX LGA1700 DDR5', N'5909002000245', N'DDR5, Wi-Fi 6.', N'szt.', N'Płyty główne', 640.00, 879.00, 12, 48, 1),
            (N'CZ-MB-Z790', N'Płyta główna Z790 ATX LGA1700 DDR5', N'5909002000252', N'DDR5, overclocking, 2,5 GbE.', N'szt.', N'Płyty główne', 1050.00, 1399.00, 5, 20, 1),
            (N'CZ-RAM-D4-8', N'Pamięć RAM DDR4 8 GB 3200 MHz', N'5909002000269', N'Moduł DIMM CL16.', N'szt.', N'Pamięci RAM', 75.00, 129.00, 30, 120, 1),
            (N'CZ-RAM-D4-16', N'Pamięć RAM DDR4 16 GB 3200 MHz', N'5909002000276', N'Moduł DIMM CL16.', N'szt.', N'Pamięci RAM', 135.00, 219.00, 30, 120, 1),
            (N'CZ-RAM-D5-16', N'Pamięć RAM DDR5 16 GB 5600 MHz', N'5909002000283', N'Moduł DIMM CL40.', N'szt.', N'Pamięci RAM', 185.00, 299.00, 40, 160, 1),
            (N'CZ-RAM-D5-32', N'Pamięć RAM DDR5 32 GB 5600 MHz', N'5909002000290', N'Moduł DIMM CL40.', N'szt.', N'Pamięci RAM', 360.00, 569.00, 20, 80, 1),
            (N'CZ-RAM-D5-32K', N'Zestaw pamięci DDR5 32 GB (2x16) 6000 MHz CL30', N'5909002000306', N'Zestaw dwukanałowy z profilem EXPO/XMP.', N'kpl.', N'Pamięci RAM', 420.00, 649.00, 15, 60, 1),
            (N'CZ-RAM-D5-64K', N'Zestaw pamięci DDR5 64 GB (2x32) 6000 MHz CL30', N'5909002000313', N'Zestaw dwukanałowy.', N'kpl.', N'Pamięci RAM', 820.00, 1249.00, 6, 24, 1),
            (N'CZ-RAM-SO-D5-16', N'Pamięć RAM SO-DIMM DDR5 16 GB 4800 MHz', N'5909002000320', N'Moduł do laptopów.', N'szt.', N'Pamięci RAM', 200.00, 319.00, 10, 40, 1),
            (N'CZ-SSD-500', N'Dysk SSD NVMe 500 GB Gen3', N'5909002000337', N'M.2 2280.', N'szt.', N'Dyski SSD i HDD', 140.00, 229.00, 25, 100, 1),
            (N'CZ-SSD-1T', N'Dysk SSD NVMe 1 TB Gen4', N'5909002000344', N'M.2 2280, odczyt do 7000 MB/s.', N'szt.', N'Dyski SSD i HDD', 299.00, 459.00, 40, 160, 1),
            (N'CZ-SSD-2T', N'Dysk SSD NVMe 2 TB Gen4', N'5909002000351', N'M.2 2280.', N'szt.', N'Dyski SSD i HDD', 560.00, 849.00, 20, 80, 1),
            (N'CZ-SSD-4T', N'Dysk SSD NVMe 4 TB Gen4', N'5909002000368', N'M.2 2280.', N'szt.', N'Dyski SSD i HDD', 1150.00, 1649.00, 5, 20, 1),
            (N'CZ-SSD-SATA-1T', N'Dysk SSD SATA 2,5" 1 TB', N'5909002000375', N'SATA III.', N'szt.', N'Dyski SSD i HDD', 240.00, 379.00, 15, 60, 1),
            (N'CZ-HDD-2T', N'Dysk HDD 3,5" 2 TB 7200 rpm', N'5909002000382', N'SATA III, 256 MB cache.', N'szt.', N'Dyski SSD i HDD', 250.00, 389.00, 10, 40, 1),
            (N'CZ-HDD-4T', N'Dysk HDD 3,5" 4 TB 7200 rpm', N'5909002000399', N'SATA III.', N'szt.', N'Dyski SSD i HDD', 420.00, 649.00, 10, 40, 1),
            (N'CZ-HDD-8T-NAS', N'Dysk HDD 3,5" 8 TB do NAS', N'5909002000405', N'Seria NAS, 24/7.', N'szt.', N'Dyski SSD i HDD', 780.00, 1149.00, 4, 16, 1),
            (N'CZ-PSU-550B', N'Zasilacz ATX 550 W 80 Plus Bronze', N'5909002000412', N'Zasilacz niemodularny.', N'szt.', N'Zasilacze', 150.00, 249.00, 10, 40, 1),
            (N'CZ-PSU-650G', N'Zasilacz ATX 650 W 80 Plus Gold', N'5909002000429', N'Zasilacz modularny.', N'szt.', N'Zasilacze', 250.00, 399.00, 15, 60, 1),
            (N'CZ-PSU-750G', N'Zasilacz ATX 750 W 80 Plus Gold', N'5909002000436', N'Zasilacz modularny.', N'szt.', N'Zasilacze', 310.00, 479.00, 12, 48, 1),
            (N'CZ-PSU-850G3', N'Zasilacz ATX 850 W 80 Plus Gold ATX 3.0', N'5909002000443', N'Złącze 12VHPWR, modularny.', N'szt.', N'Zasilacze', 410.00, 629.00, 8, 32, 1),
            (N'CZ-PSU-1000P', N'Zasilacz ATX 1000 W 80 Plus Platinum', N'5909002000450', N'Zasilacz w pełni modularny.', N'szt.', N'Zasilacze', 680.00, 999.00, 4, 16, 1),
            (N'CZ-CASE-ITX', N'Obudowa Mini-ITX z uchwytem', N'5909002000467', N'Obudowa kompaktowa.', N'szt.', N'Obudowy', 280.00, 449.00, 4, 16, 1),
            (N'CZ-CASE-MATX', N'Obudowa mATX z szybą', N'5909002000474', N'Dwie szyby, 1 wentylator.', N'szt.', N'Obudowy', 170.00, 279.00, 10, 40, 1),
            (N'CZ-CASE-ATX', N'Obudowa Midi Tower ATX airflow', N'5909002000481', N'Przedni panel siatkowy, 3 wentylatory.', N'szt.', N'Obudowy', 230.00, 369.00, 14, 56, 1),
            (N'CZ-CASE-ATXW', N'Obudowa Midi Tower ATX biała', N'5909002000498', N'Białe wykończenie, szkło hartowane.', N'szt.', N'Obudowy', 260.00, 419.00, 8, 32, 1),
            (N'CZ-CASE-FULL', N'Obudowa Full Tower E-ATX', N'5909002000504', N'Obsługa chłodzenia 360 mm.', N'szt.', N'Obudowy', 520.00, 829.00, 3, 12, 1),
            (N'CZ-COOL-T120', N'Chłodzenie CPU wieża 120 mm', N'5909002000511', N'Tower z jednym wentylatorem.', N'szt.', N'Chłodzenie', 65.00, 119.00, 15, 60, 1),
            (N'CZ-COOL-D120', N'Chłodzenie CPU dual-tower 2x120 mm', N'5909002000528', N'Do procesorów do 250 W.', N'szt.', N'Chłodzenie', 210.00, 339.00, 8, 32, 1),
            (N'CZ-COOL-AIO240', N'Chłodzenie wodne AIO 240 mm', N'5909002000535', N'Radiator 240 mm, ARGB.', N'szt.', N'Chłodzenie', 260.00, 429.00, 8, 32, 1),
            (N'CZ-COOL-AIO360', N'Chłodzenie wodne AIO 360 mm', N'5909002000542', N'Radiator 360 mm.', N'szt.', N'Chłodzenie', 390.00, 649.00, 5, 20, 1),
            (N'CZ-FAN-120-3', N'Wentylator 120 mm PWM (zestaw 3 szt.)', N'5909002000559', N'Zestaw trzech wentylatorów z kontrolerem.', N'kpl.', N'Chłodzenie', 55.00, 109.00, 15, 60, 1),
            (N'CZ-PASTE-4G', N'Pasta termoprzewodząca 4 g', N'5909002000566', N'Strzykawka 4 g.', N'szt.', N'Chłodzenie', 9.00, 24.00, 40, 160, 1),
            (N'CZ-MON24-FHD', N'Monitor 24" FHD IPS 165 Hz', N'5909002000573', N'Matryca IPS 165 Hz, 1 ms.', N'szt.', N'Monitory', 420.00, 649.00, 10, 40, 1),
            (N'CZ-MON27-QHD', N'Monitor 27" QHD IPS 170 Hz', N'5909002000580', N'Matryca IPS 170 Hz, HDR.', N'szt.', N'Monitory', 790.00, 1190.00, 8, 32, 1),
            (N'CZ-MON27-4K', N'Monitor 27" 4K IPS USB-C', N'5909002000597', N'Hub USB-C 90 W.', N'szt.', N'Monitory', 1250.00, 1799.00, 4, 16, 1),
            (N'CZ-MON32-4K', N'Monitor 32" 4K VA', N'5909002000603', N'Matryca VA, FreeSync.', N'szt.', N'Monitory', 1450.00, 2099.00, 3, 12, 1),
            (N'CZ-MON34-UW', N'Monitor 34" UWQHD zakrzywiony 165 Hz', N'5909002000610', N'Format 21:9, zakrzywienie 1500R.', N'szt.', N'Monitory', 1680.00, 2449.00, 3, 12, 1),
            (N'CZ-KBD-MEMB', N'Klawiatura przewodowa membranowa układ PL', N'5909002000627', N'Pełnowymiarowa, USB.', N'szt.', N'Klawiatury i myszy', 32.00, 59.00, 25, 100, 1),
            (N'CZ-KBD-TKL', N'Klawiatura mechaniczna TKL układ PL', N'5909002000634', N'Przełączniki liniowe, podświetlenie.', N'szt.', N'Klawiatury i myszy', 165.00, 299.00, 10, 40, 1),
            (N'CZ-KBD-FULL', N'Klawiatura mechaniczna pełnowymiarowa', N'5909002000641', N'Hot-swap, podświetlenie RGB.', N'szt.', N'Klawiatury i myszy', 210.00, 369.00, 8, 32, 1),
            (N'CZ-MSE-OPT', N'Mysz optyczna przewodowa', N'5909002000658', N'1600 DPI.', N'szt.', N'Klawiatury i myszy', 18.00, 39.00, 40, 160, 1),
            (N'CZ-MSE-GAME', N'Mysz bezprzewodowa dla graczy 26000 DPI', N'5909002000665', N'Sensor optyczny, 70 h pracy.', N'szt.', N'Klawiatury i myszy', 190.00, 329.00, 10, 40, 1),
            (N'CZ-KBD-MSE-WL', N'Zestaw bezprzewodowy klawiatura + mysz', N'5909002000672', N'Zestaw biurowy 2,4 GHz.', N'kpl.', N'Klawiatury i myszy', 60.00, 119.00, 20, 80, 1),
            (N'CZ-NIC-25G', N'Karta sieciowa PCIe 2,5 GbE', N'5909002000689', N'Chipset Intel 2,5 GbE.', N'szt.', N'Sieć i karty rozszerzeń', 45.00, 89.00, 15, 60, 1),
            (N'CZ-NIC-10G', N'Karta sieciowa PCIe 10 GbE SFP+', N'5909002000696', N'Złącze SFP+, niski profil w zestawie.', N'szt.', N'Sieć i karty rozszerzeń', 310.00, 529.00, 4, 16, 1),
            (N'CZ-WIFI6E', N'Karta Wi-Fi 6E PCIe z Bluetooth', N'5909002000702', N'Dwie anteny, Bluetooth 5.3.', N'szt.', N'Sieć i karty rozszerzeń', 70.00, 139.00, 12, 48, 1),
            (N'CZ-USB-CTRL', N'Kontroler USB 3.2 PCIe 4 porty', N'5909002000719', N'Cztery porty USB-A.', N'szt.', N'Sieć i karty rozszerzeń', 55.00, 109.00, 8, 32, 1),
            (N'CZ-SW8-25G', N'Switch 8-portowy 2,5 GbE niezarządzalny', N'5909002000726', N'Obudowa metalowa.', N'szt.', N'Sieć i karty rozszerzeń', 190.00, 329.00, 6, 24, 1),
            (N'CZ-RTR-W6', N'Router Wi-Fi 6 AX3000', N'5909002000733', N'Dwuzakresowy, 4 porty LAN.', N'szt.', N'Sieć i karty rozszerzeń', 210.00, 359.00, 10, 40, 1),
            (N'CZ-KAB-DP-2M', N'Kabel DisplayPort 1.4 2 m', N'5909002000740', N'Kabel do monitorów 4K.', N'szt.', N'Kable i akcesoria', 16.00, 35.00, 30, 120, 1),
            (N'CZ-KAB-HDMI-2M', N'Kabel HDMI 2.1 2 m', N'5909002000757', N'8K/60 Hz.', N'szt.', N'Kable i akcesoria', 14.00, 30.00, 30, 120, 1),
            (N'CZ-KAB-SATA-4', N'Kable SATA data (4 szt.)', N'5909002000764', N'Kable SATA III z zatrzaskami.', N'op.', N'Kable i akcesoria', 9.00, 22.00, 30, 120, 1),
            (N'CZ-KAB-PWR-IEC', N'Kabel zasilający IEC C13 1,8 m', N'5909002000771', N'Kabel zasilający do zasilaczy i monitorów.', N'szt.', N'Kable i akcesoria', 7.00, 19.00, 40, 160, 1),
            (N'CZ-KAB-ATX-EXT', N'Przedłużacz ATX 24-pin w oplocie', N'5909002000788', N'Oplot nylonowy.', N'szt.', N'Kable i akcesoria', 28.00, 65.00, 10, 40, 1),
            (N'CZ-KAB-USBC-1M', N'Kabel USB-C – USB-C 100 W 1 m', N'5909002000795', N'USB 3.2, 100 W.', N'szt.', N'Kable i akcesoria', 12.00, 28.00, 40, 160, 1),
            (N'CZ-ACC-M2-SCREW', N'Zestaw śrub i podkładek M.2', N'5909002000801', N'Śruby mocujące dyski M.2.', N'op.', N'Kable i akcesoria', 3.00, 9.00, 50, 200, 1),
            (N'CZ-ACC-ESD', N'Opaska antystatyczna ESD', N'5909002000818', N'Z przewodem uziemiającym.', N'szt.', N'Kable i akcesoria', 5.00, 15.00, 25, 100, 1),
            (N'CZ-ACC-VELCRO-M', N'Taśma rzep do kabli 20 mm (na metry)', NULL, N'Sprzedaż na metry z rolki.', N'm', N'Kable i akcesoria', 1.60, 3.20, 100, 400, 1),
            (N'CZ-PC-GAME-R5', N'Komputer gamingowy Ryzen 5 / RTX 4060 / 16 GB', N'5909002000825', N'Zestaw gotowy do pracy, Windows 11.', N'kpl.', N'Komputery i laptopy', 3450.00, 4699.00, 3, 12, 1),
            (N'CZ-PC-OFFICE-I5', N'Komputer biurowy i5 / 16 GB / 512 GB', N'5909002000832', N'Zestaw biurowy z klawiaturą i myszą.', N'kpl.', N'Komputery i laptopy', 1850.00, 2599.00, 5, 20, 1),
            (N'CZ-PC-MINI', N'Mini PC i5 / 16 GB / 512 GB', N'5909002000849', N'Komputer kompaktowy.', N'szt.', N'Komputery i laptopy', 2250.00, 3190.00, 4, 16, 1),
            (N'CZ-NB-15-R7', N'Laptop 15,6" Ryzen 7 / 16 GB / 1 TB', N'5909002000856', N'Matryca IPS FHD.', N'szt.', N'Komputery i laptopy', 3250.00, 4390.00, 4, 16, 1),
            (N'CZ-NB-14-BIZ', N'Laptop biznesowy 14" i5 / 16 GB / 512 GB', N'5909002000863', N'Windows 11 Pro, 3 lata gwarancji.', N'szt.', N'Komputery i laptopy', 3150.00, 4290.00, 5, 20, 1)
        ) AS seed (SKU, Name, Barcode, Description, UnitSymbol, CategoryName, PurchasePrice, SalePrice, MinimumQuantity, OptimumQuantity, IsActive)
        INNER JOIN dbo.UnitsOfMeasure AS unitOfMeasure ON unitOfMeasure.Symbol = seed.UnitSymbol
        INNER JOIN dbo.Categories AS category ON category.Name = seed.CategoryName
    ) AS source
        ON target.SKU = source.SKU
    WHEN MATCHED THEN
        UPDATE SET
            Name = source.Name,
            Barcode = source.Barcode,
            Description = source.Description,
            UnitOfMeasureId = source.UnitOfMeasureId,
            CategoryId = source.CategoryId,
            PurchasePrice = source.PurchasePrice,
            SalePrice = source.SalePrice,
            MinimumQuantity = source.MinimumQuantity,
            OptimumQuantity = source.OptimumQuantity,
            IsActive = source.IsActive
    WHEN NOT MATCHED THEN
        INSERT (Id, Name, SKU, Barcode, Description, UnitOfMeasureId, CategoryId,
                PurchasePrice, SalePrice, MinimumQuantity, OptimumQuantity, IsActive)
        VALUES (NEWID(), source.Name, source.SKU, source.Barcode, source.Description, source.UnitOfMeasureId, source.CategoryId,
                source.PurchasePrice, source.SalePrice, source.MinimumQuantity, source.OptimumQuantity, source.IsActive);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

---------------------------------------------------------------------------
-- Kontrola wyniku
---------------------------------------------------------------------------
SELECT category.Name AS Kategoria,
       COUNT(*) AS Produkty,
       SUM(CASE WHEN product.IsActive = 1 THEN 1 ELSE 0 END) AS Aktywne
FROM dbo.Products AS product
INNER JOIN dbo.Categories AS category ON category.Id = product.CategoryId
WHERE product.SKU LIKE 'CZ-%'
GROUP BY category.Name
ORDER BY category.Name;
GO
