/*
  Uzupełnia dane widoczne na wydrukach dokumentów (WZ, zamówienia zewnętrzne, wydania do klienta):
  pełne adresy i NIP magazynów oraz adresy dostawców, którzy ich nie mają.

  Wymaga migracji DocumentDetails (kolumna Warehouses.TaxNumber). Skrypt można uruchamiać wielokrotnie
  i uzupełnia tylko puste pola albo adresy zapisane samą nazwą miasta; ręcznie wpisanych danych nie nadpisuje.
  Wszystkie dane są fikcyjne, służą wyłącznie do demonstracji.

  sqlcmd -C -I -f 65001 -S localhost -d Magazine -U sa -P "<haslo>" -i seeds\seed-document-details.sql
*/
SET NOCOUNT ON;

UPDATE dbo.Warehouses
SET Address = N'ul. Portowa 14, 80-860 Gdańsk'
WHERE Name LIKE N'%Gda%' AND (Address IS NULL OR Address IN (N'Gdańsk', N'Gdańsk, Port'));

UPDATE dbo.Warehouses
SET Address = N'ul. Magazynowa 7, 30-701 Kraków'
WHERE Name LIKE N'%Krak%' AND (Address IS NULL OR Address = N'Kraków');

UPDATE dbo.Warehouses
SET Address = N'ul. Annopol 20, 03-236 Warszawa'
WHERE Name LIKE N'%Warszaw%' AND (Address IS NULL OR Address = N'Warszawa');

UPDATE dbo.Warehouses
SET Address = N'ul. Gdyńska 33, 61-016 Poznań'
WHERE Name LIKE N'%Pozna%' AND (Address IS NULL OR Address = N'Poznań');

-- Jedna firma prowadzi całą sieć magazynów, więc wszystkie oddziały mają ten sam NIP.
UPDATE dbo.Warehouses SET TaxNumber = N'584-000-00-01' WHERE TaxNumber IS NULL;

UPDATE dbo.Contractors
SET Address = N'ul. Elektroniczna 5, 00-950 Warszawa'
WHERE Address IS NULL AND Type IN (N'Supplier', N'Both');


SELECT N'Magazyny' AS Tabela, Name, Address, TaxNumber FROM dbo.Warehouses ORDER BY Name;
