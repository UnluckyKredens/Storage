# Magazine v2 - dokumentacja do pracy inzynierskiej

Ten plik jest jedna wspolna dokumentacja projektu. Zastapil poprzednie rozproszone pliki `.md`.

## Cel projektu

Magazine v2 to system magazynowy do obslugi czesci komputerowych. Projekt pokazuje pelny przeplyw pracy magazynu:

- katalog produktow z opisem, zdjeciem, SKU i kodem kreskowym,
- stany magazynowe w lokalizacjach,
- rezerwacje towaru,
- wysylki miedzy magazynami,
- zamowienia zewnetrzne od dostawcow,
- zakup przez publiczna biblioteke `sw.js`,
- automatyczny wybor magazynu do spakowania zakupu,
- panel "Zakupy do spakowania",
- historia ruchow magazynowych,
- role, uprawnienia i uzytkownicy,
- bot symulujacy prace magazynu.

## Sklad projektu

- `MagazineAPI/MagazineAPI` - aplikacja ASP.NET Core, kontrolery HTTP, autoryzacja, plik `sw.js`.
- `MagazineAPI/MagazineAPIApplication` - logika przypadkow uzycia, komendy, zapytania.
- `MagazineAPI/MagazineAPIDomain` - encje, statusy, stale uprawnien.
- `MagazineAPI/MagazineAPIInfrastructure` - Entity Framework Core, migracje, konfiguracje tabel.
- `MagazineAPI/MagazineAPIEvent` - zdarzenia i zapisy historii.
- `Magazine-SPA` - panel Angular dla pracownikow i kierownikow.
- `MagazineWarehouseBot` - bot do symulowania pracy magazynu.
- `StorefrontDemo` - gotowa strona testowa korzystajaca z `sw.js`.
- `scripts/seed-production-computer-parts.sql` - seed czesci komputerowych.

## Technologie

- Backend: .NET 10, ASP.NET Core, Entity Framework Core, SQL Server.
- Frontend: Angular 21, Angular Material.
- Baza danych: SQL Server oraz osobna baza historii.
- Integracja publiczna: `sw.js` jako uniwersalna biblioteka klienta API.
- Symulacja: osobna aplikacja konsolowa `MagazineWarehouseBot`.

## Najwazniejsze moduly

### Produkty

Produkty maja nazwe, SKU, kod kreskowy, opis, zdjecie, cene zakupu, cene sprzedazy i progi zapasu.

Najwazniejsze pliki:

- `MagazineAPI/MagazineAPIDomain/Entities/Product.cs`
- `MagazineAPI/MagazineAPIApplication/Modules/Products`
- `Magazine-SPA/src/app/management/pages/products`

### Stany magazynowe

Stan jest trzymany w tabeli `Inventory`. Kazdy rekord laczy produkt z lokalizacja. Dostepna ilosc to `Quantity - ReservedQuantity`.

Najwazniejsze pliki:

- `MagazineAPI/MagazineAPIDomain/Entities/Inventory.cs`
- `MagazineAPI/MagazineAPIInfrastructure/Persistence/Configurations/InventoryConfiguration.cs`
- `Magazine-SPA/src/app/management/pages/inventory`

### Wysylki

Wysylki obsluguja ruch towaru miedzy magazynami. Kierownik moze zaakceptowac wysylke, a magazyn docelowy moze ja przyjac.

Najwazniejsze pliki:

- `MagazineAPI/MagazineAPI/Controllers/ShipmentsController.cs`
- `MagazineAPI/MagazineAPIApplication/Modules/Shipments`
- `Magazine-SPA/src/app/management/pages/shipments`

### Zamowienia zewnetrzne

Zamowienia zewnetrzne sa dla dostaw od firm. Pracownik lub kierownik tworzy zamowienie, kierownik je akceptuje, podaje fakture i dokument papierowy, a potem przyjmuje towar na stan.

Najwazniejsze pliki:

- `MagazineAPI/MagazineAPI/Controllers/PurchaseOrdersController.cs`
- `MagazineAPI/MagazineAPIApplication/Modules/PurchaseOrders`
- `Magazine-SPA/src/app/management/pages/purchase-orders`

### Zakupy przez `sw.js`

`sw.js` nie renderuje gotowego sklepu. Jest uniwersalna biblioteka, ktora pobiera produkty, sklada zamowienia i sprawdza status. Strona, sklep albo inna aplikacja sama decyduje jak pokazac produkty, koszyk i formularz zakupu. Po zakupie backend sam wybiera magazyn, ktory moze spakowac cale zamowienie na podstawie aktualnego stanu.

Przyklad uzycia:

```html
<script src="http://localhost:5292/sw.js"></script>
<script>
  async function runStorefront() {
    const store = window.MagazineStore.create({
      apiUrl: 'http://localhost:5292/api/Storefront'
    });

    const products = await store.getProducts();
    const order = await store.createOrder({
      customerName: 'Jan Kowalski',
      customerEmail: 'jan@example.com',
      items: [
        { productId: products[0].id, quantity: 1 }
      ]
    });
    const tracking = await store.getTracking(order.number);
  }

  runStorefront();
</script>
```

Najwazniejsze pliki:

- `MagazineAPI/MagazineAPI/wwwroot/sw.js`
- `MagazineAPI/MagazineAPI/Controllers/StorefrontController.cs`
- `MagazineAPIDomain/Entities/StorefrontOrder.cs`
- `MagazineAPIDomain/Entities/StorefrontOrderAllocation.cs`
- `StorefrontDemo/index.html`

### Zakupy do spakowania

To panel magazynowy dla zamowien z publicznego sklepu. Zamowienie ma status:

- `Nowe` - zakup zapisany, czeka na obsluge,
- `Przyjete do realizacji` - magazyn kompletuje paczke,
- `Zrealizowane` - paczka spakowana i zdjeta ze stanu,
- `Anulowane` - zamowienie zamkniete.

Najwazniejsze pliki:

- `MagazineAPI/MagazineAPI/Controllers/StorefrontController.cs`
- `Magazine-SPA/src/app/management/pages/storefront-packing`
- `Magazine-SPA/src/app/management/services/storefront-packing.service.ts`

### Bot magazynowy

Bot loguje sie do API i symuluje operacje: tworzenie stanow, wysylek i zamowien.

Najwazniejsze pliki:

- `MagazineWarehouseBot/Simulation/WarehouseBotService.cs`
- `MagazineWarehouseBot/Api/MagazineApiClient.cs`
- `MagazineWarehouseBot/appsettings.json`

## Newralgiczne miejsca w kodzie

### Autoryzacja i uprawnienia

Uprawnienia sa sprawdzane przez atrybut `HasPermission`.

Pliki:

- `MagazineAPI/MagazineAPI/Authorization`
- `MagazineAPI/MagazineAPIDomain/Authorization/PermissionCodes.cs`
- `MagazineAPI/MagazineAPIDomain/Authorization/PermissionIds.cs`
- `MagazineAPI/MagazineAPIInfrastructure/Persistence/Configurations/RolePermissionConfiguration.cs`

Ryzyko: gdy dodasz nowy modul, trzeba dodac uprawnienie, przypisac je roli i uzyc guarda w Angularze.

### Rezerwacje i dostepny stan

Nie mozna patrzec tylko na `Quantity`. Trzeba patrzec na `AvailableQuantity`, czyli ilosc minus rezerwacje.

Pliki:

- `InventoryConfiguration.cs`
- `StockReservation.cs`
- `StorefrontOrderAllocation.cs`
- `StorefrontController.cs`

Ryzyko: bledne zdjecie ze stanu moze pozwolic sprzedac ten sam towar dwa razy.

### Wybor magazynu do pakowania

Algorytm znajduje jeden magazyn, ktory ma wszystkie produkty z koszyka. Potem rezerwuje konkretne lokalizacje.

Plik:

- `MagazineAPI/MagazineAPI/Controllers/StorefrontController.cs`

Ryzyko: przy duzym ruchu trzeba uwazac na rownolegle zakupy. Transakcja i rezerwacja ograniczaja problem, ale w pracy mozna wskazac to jako miejsce do dalszego rozwoju.

### Migracje bazy

Migracje sa w:

- `MagazineAPI/MagazineAPIInfrastructure/Migrations`
- `MagazineAPI/MagazineAPIInfrastructure/Migrations/History`

Ryzyko: po zmianie encji trzeba wygenerowac migracje i sprawdzic snapshot.

### Konfiguracja produkcyjna

Nie wolno uzywac produkcyjnie hasel z `appsettings.json`. Wartosci lokalne sa tylko do developmentu.

Najwazniejsze ustawienia:

- `ConnectionStrings__DefaultConnection`
- `ConnectionStrings__HistoryConnection`
- `Jwt__SecretKey`
- `InitialAdmin__Password`
- `ASPNETCORE_ENVIRONMENT=Production`

## Jak uruchomic lokalnie

### 1. SQL Server

Jezeli kontener juz istnieje:

```bash
docker start sqlserver
```

Jezeli trzeba go utworzyc:

```bash
docker run -d \
  --name sqlserver \
  -e ACCEPT_EULA=Y \
  -e MSSQL_SA_PASSWORD='Sernik2025!' \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

### 2. RabbitMQ

Jezeli kontener juz istnieje:

```bash
docker start rabbitmq
```

Jezeli trzeba go utworzyc:

```bash
docker run -d \
  --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management-alpine
```

### 3. Migracje bazy

```bash
dotnet ef database update \
  --project MagazineAPI/MagazineAPIInfrastructure/MagazineAPInfrastructure.csproj \
  --startup-project MagazineAPI/MagazineAPI/MagazineAPI.csproj \
  --context AppDbContext

dotnet ef database update \
  --project MagazineAPI/MagazineAPIInfrastructure/MagazineAPInfrastructure.csproj \
  --startup-project MagazineAPI/MagazineAPI/MagazineAPI.csproj \
  --context HistoryDbContext
```

### 4. Seed czesci komputerowych

```bash
docker exec -i sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P 'Sernik2025!' \
  -C \
  -d Magazine \
  < scripts/seed-production-computer-parts.sql
```

### 5. Backend

```bash
dotnet run --project MagazineAPI/MagazineAPI/MagazineAPI.csproj
```

Adres API:

```text
http://localhost:5292
```

Swagger:

```text
http://localhost:5292/swagger
```

### 6. Frontend Angular

```bash
cd Magazine-SPA
npm install
npm start
```

### 7. Demo sklepu

```bash
cd StorefrontDemo
python3 -m http.server 8088
```

Adres:

```text
http://localhost:8088
```

## Jak postawic instancje produkcyjna

1. Przygotuj SQL Server i dwie bazy: `Magazine`, `MagazineHistory`.
2. Ustaw connection stringi jako zmienne srodowiskowe.
3. Ustaw mocny `Jwt__SecretKey`.
4. Ustaw tymczasowe haslo `InitialAdmin__Password`.
5. Wykonaj migracje EF.
6. Uruchom seed, jezeli chcesz startowe czesci komputerowe.
7. Uruchom API w trybie `Production`.
8. Zaloguj sie administratorem.
9. Zmien haslo administratora.
10. Utworz konta kierownikow i pracownikow.
11. Przypisz pracownikom magazyny.
12. Uruchom frontend z adresem API produkcyjnego.
13. Nie uruchamiaj bota na produkcji, chyba ze jest to srodowisko testowe.

Przykladowe zmienne:

```bash
ConnectionStrings__DefaultConnection="Server=tcp:SQL_HOST,1433;Database=Magazine;User Id=APP_USER;Password=STRONG_PASSWORD;Encrypt=True;TrustServerCertificate=False"
ConnectionStrings__HistoryConnection="Server=tcp:SQL_HOST,1433;Database=MagazineHistory;User Id=APP_USER;Password=STRONG_PASSWORD;Encrypt=True;TrustServerCertificate=False"
Jwt__SecretKey="DLUGI_LOSOWY_SEKRET_MINIMUM_32_ZNAKI"
InitialAdmin__Enabled="true"
InitialAdmin__Login="admin"
InitialAdmin__Password="TYMCZASOWE_MOCNE_HASLO"
ASPNETCORE_ENVIRONMENT="Production"
```

## Jak sprawdzic, czy dziala

Backend:

```bash
dotnet build MagazineAPI/MagazineAPI/MagazineAPI.csproj --no-restore
```

Frontend:

```bash
cd Magazine-SPA
npm run build
```

Baza:

```sql
SELECT COUNT(*) FROM dbo.Products;
SELECT COUNT(*) FROM dbo.Inventory;
SELECT COUNT(*) FROM dbo.StorefrontOrders;
```

Biblioteka `sw.js`:

```text
GET http://localhost:5292/sw.js
GET http://localhost:5292/api/Storefront/products
```

## Jak przestudiowac kod po kolei

1. Zacznij od encji domenowych:
   - `Product.cs`
   - `Warehouse.cs`
   - `Location.cs`
   - `Inventory.cs`
   - `Shipment.cs`
   - `PurchaseOrder.cs`
   - `StorefrontOrder.cs`

2. Potem zobacz konfiguracje EF:
   - `MagazineAPIInfrastructure/Persistence/Configurations`
   - `AppDbContext.cs`

3. Nastepnie przejdz do migracji:
   - `MagazineAPIInfrastructure/Migrations`
   - `MagazineAPIInfrastructure/Migrations/History`

4. Potem przeczytaj kontrolery API:
   - `ProductsController`
   - `InventoriesController`
   - `ShipmentsController`
   - `PurchaseOrdersController`
   - `StorefrontController`

5. Pozniej przejdz do logiki aplikacyjnej:
   - `MagazineAPIApplication/Modules/Products`
   - `MagazineAPIApplication/Modules/Shipments`
   - `MagazineAPIApplication/Modules/PurchaseOrders`
   - `MagazineAPIApplication/Modules/WarehouseOperations`

6. Nastepnie sprawdz frontend:
   - `management.routes.ts`
   - `management-container.component.ts`
   - strony w `Magazine-SPA/src/app/management/pages`
   - serwisy w `Magazine-SPA/src/app/management/services`

7. Na koncu sprawdz integracje zewnetrzne:
   - `sw.js`
   - `StorefrontDemo`
   - `MagazineWarehouseBot`

## Scenariusz pokazowy do pracy inzynierskiej

1. Zaloguj sie jako administrator.
2. Pokaz katalog produktow z opisem i zdjeciem.
3. Pokaz stany magazynowe i lokalizacje.
4. Zloz zakup przez `StorefrontDemo`.
5. Pokaz, ze zamowienie dostalo magazyn odpowiedzialny za pakowanie.
6. Wejdz w `Zakupy do spakowania`.
7. Kliknij `Akceptuj`.
8. Kliknij `Spakuj`.
9. Pokaz, ze stan magazynowy zostal zmniejszony.
10. Pokaz sledzenie zamowienia po numerze.
11. Pokaz zamowienie zewnetrzne do dostawcy.
12. Pokaz przyjecie dostawy na stan.

## Proste wyjasnienie algorytmu wyboru magazynu

1. System bierze produkty z koszyka.
2. Sprawdza stany we wszystkich magazynach.
3. Dla kazdego magazynu liczy, czy ma komplet produktow.
4. Wybiera magazyn, ktory ma komplet i najmniejsza nadwyzke.
5. Rezerwuje konkretne lokalizacje w tym magazynie.
6. Pracownik pakuje zamowienie z tych lokalizacji.
7. Po spakowaniu system zdejmuje towar ze stanu.

## Dane testowe

Seed dodaje:

- 30 czesci komputerowych,
- 12 kategorii,
- 8 kontrahentow,
- 2 magazyny,
- 10 lokalizacji,
- role i uprawnienia.

Lokalne konto administratora tworzy `DatabaseInitializer` po starcie API, jezeli nie ma admina.

Domyslnie lokalnie:

```text
login: admin
haslo: Test123!
```

To haslo jest tylko do developmentu.
