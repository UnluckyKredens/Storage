# Magazine v2 - aktualny stan aplikacji i instrukcja przygotowania magazynu

Dokument opisuje stan kodu znajdującego się obecnie w repozytorium. Rozdział **Roadmapa** zawiera funkcje planowane, których nie należy traktować jako dostępnych w bieżącej wersji.

## 1. Przeznaczenie i architektura

Magazine v2 jest systemem do obsługi katalogu towarów, magazynów, lokalizacji, stanów, kontrahentów, użytkowników oraz dokumentów PZ i WZ.

- API: ASP.NET Core na .NET 10.
- Baza: SQL Server, Entity Framework Core i migracje EF Core.
- Uwierzytelnianie: JWT Bearer.
- Autoryzacja: role i uprawnienia `HasPermission`; administrator ma pełny dostęp.
- Frontend: Angular 21, Angular Material, RxJS i NgRx Signals.
- REST/JSON: prefiks `/api`.
- Warstwy: `MagazineAPI` (HTTP), `MagazineAPIApplication` (komendy/zapytania), `MagazineAPIDomain` (encje/reguły), `MagazineAPIInfrastructure` (EF Core/repozytoria).

Encje: `Category`, `Contractor`, `Inventory`, `Location`, `Permission`, `Product`, `Role`, `RolePermission`, `StockDocument`, `StockDocumentItem`, `UnitOfMeasure`, `User`, `Warehouse`.

Wszystkie bieżące identyfikatory i klucze obce są typu `Guid` (`uniqueidentifier`). `RolePermission` ma klucz złożony `(RoleId, PermissionId)`. `Inventory.AvailableQuantity` jest kolumną wyliczaną: `Quantity - ReservedQuantity`.

## 2. Zakres magazynu i dokumenty

Każde żądanie poza logowaniem wymaga JWT. Zakres danych ogranicza `IWarehouseContext`:

- administrator może pracować na wszystkich magazynach;
- każdy inny użytkownik musi mieć dokładnie jeden przypisany magazyn;
- zwykły użytkownik nie może pracować na danych obcego magazynu;
- administrator wskazuje aktywny magazyn nagłówkiem `X-Warehouse-Id`;
- kontrola API działa niezależnie od ograniczeń w SPA.

`Receipt` to PZ, a `Shipment` to WZ. Statusy to `Draft`, `Completed` i `Received`. Szkic można edytować i usunąć. Zatwierdzenie zmienia `Inventory` i przenosi dokument do historii. Odbiór PZ zapisuje odbierającego i czas, ale sam nie księguje stanu.

Przebieg PZ: szkic -> opcjonalny odbiór skanem -> zatwierdzenie -> wzrost stanu.

Przebieg WZ: szkic wysyłki -> zatwierdzenie -> zmniejszenie stanu magazynu źródłowego.

Księgowanie jest transakcyjne. Po zatwierdzeniu dokumentu nie wolno go edytować ani usuwać. Bezpośrednia edycja `Inventory` jest jednak nadal możliwa z `inventory.manage`.

## 3. Role i uprawnienia

| Rola | Zakres |
|---|---|
| `Administrator` | Pełny dostęp, bez magazynu; zarządza wszystkimi oddziałami, rolami i uprawnieniami. |
| `Kierownik` | Przypisany magazyn; zarządza dokumentami i może je zatwierdzać. |
| `Pracownik` | Odczyt produktów i stanów, odbiór PZ skanem, tworzenie szkiców wysyłek. |

| Uprawnienie | Znaczenie |
|---|---|
| `products.read`, `products.manage` | Odczyt oraz CRUD produktów. |
| `inventory.read`, `inventory.manage` | Odczyt oraz bezpośrednia edycja stanów. |
| `warehouses.read`, `warehouses.manage` | Odczyt oraz CRUD magazynów i lokalizacji. |
| `contractors.read`, `contractors.manage` | Odczyt oraz CRUD kontrahentów. |
| `users.read`, `users.manage` | Odczyt oraz zarządzanie kontami. |
| `dictionaries.manage` | Kategorie i jednostki miary. |
| `stock-documents.read` | Listy, szczegóły i historia PZ/WZ. |
| `stock-documents.manage` | Tworzenie, edycja i usuwanie szkiców. |
| `stock-documents.approve` | Zatwierdzanie i księgowanie. |
| `stock-documents.receive` | Skanowanie i odbiór PZ. |
| `stock-shipments.create` | Tworzenie szkiców WZ między oddziałami. |
| `roles.manage` | Role, uprawnienia i przypisania; administrator. |

Administrator jest rozpoznawany po stałym identyfikatorze roli i nie potrzebuje wpisów `RolePermissions`.

## 4. API - zasady wspólne

```http
Authorization: Bearer <token>
X-Warehouse-Id: <warehouse-guid>
```

`X-Warehouse-Id` jest potrzebny administratorowi przy pracy w wybranym magazynie. Typowe odpowiedzi: `200` odczyt/edycja, `201` utworzenie, `204` usunięcie, `400` walidacja, `401` brak/nieważny JWT, `403` brak uprawnienia lub obcy magazyn, `404` brak rekordu, `409` konflikt unikalności/zależności/statusu, `500` błąd serwera. W Development działa Swagger `/swagger`, CORS jest otwarty, a API automatycznie wykonuje migracje.

## 5. Kompletna lista endpointów

### Uwierzytelnianie - `/api/Auth`

| Metoda i ścieżka | Dostęp | Działanie |
|---|---|---|
| `POST /api/Auth/login` | publiczny | Logowanie loginem lub e-mailem i hasłem; JWT. |
| `GET /api/Auth/IsAuthenticated` | JWT | Test tokenu; zwraca `true`. |
| `GET /api/Auth/me` | JWT | Dane zalogowanego użytkownika. |
| `PUT /api/Auth/me` | JWT | Edycja konta i opcjonalna zmiana hasła. |
| `GET /api/Auth/me/permissions` | JWT | Efektywne kody uprawnień. |

### Produkty - `/api/Products`

| Metoda i ścieżka | Dostęp | Działanie/opcje |
|---|---|---|
| `GET /api/Products` | `products.read` | Paginacja i parametry `page`, `pageSize`, `search`, `sortBy`, `order`. |
| `GET /api/Products/{id}` | `products.read` | Szczegóły. |
| `GET /api/Products/page-data` | `products.read` | Kategorie i jednostki formularza. |
| `POST /api/Products` | `products.manage` | Utworzenie: nazwa, SKU, barcode, opis, kategoria, jednostka, ceny, aktywność. |
| `PUT /api/Products/{id}` | `products.manage` | Edycja. |
| `DELETE /api/Products/{id}` | `products.manage` | Usunięcie bez zależnych stanów. |

### Kategorie i jednostki miary

Wszystkie trasy wymagają `dictionaries.manage`.

| Metody | Ścieżki |
|---|---|
| `GET`, `POST` | `/api/Category`, `/api/UnitsOfMeasure` |
| `GET`, `PUT`, `DELETE` | `/api/Category/{id}`, `/api/UnitsOfMeasure/{id}` |

### Magazyny i lokalizacje

Odczyt wymaga `warehouses.read`, zapis `warehouses.manage`.

| Metoda i ścieżka | Działanie |
|---|---|
| `GET /api/Warehouses` | Magazyny w zakresie użytkownika; administrator wszystkie. |
| `GET /api/Warehouses/{id}` | Szczegóły magazynu. |
| `POST /api/Warehouses` | Dodanie nazwy, adresu i opisu. |
| `PUT /api/Warehouses/{id}` | Edycja. |
| `DELETE /api/Warehouses/{id}` | Usunięcie bez lokalizacji i użytkowników. |
| `GET /api/Locations` | Lokalizacje aktywnego magazynu. |
| `GET /api/Locations/{id}` | Szczegóły. |
| `GET /api/Locations/page-data` | Magazyny do formularza lokalizacji. |
| `POST /api/Locations` | Dodanie lokalizacji. |
| `PUT /api/Locations/{id}` | Edycja kodu/opisu. |
| `DELETE /api/Locations/{id}` | Usunięcie bez stanów i zależności. |

### Kontrahenci - `/api/Contractors`

| Metoda i ścieżka | Dostęp | Działanie |
|---|---|---|
| `GET /api/Contractors`, `GET /api/Contractors/{id}` | `contractors.read` | Lista i szczegóły. |
| `POST /api/Contractors`, `PUT /api/Contractors/{id}` | `contractors.manage` | CRUD danych, typu `Supplier/Customer/Both` i kontaktu. |
| `DELETE /api/Contractors/{id}` | `contractors.manage` | Usunięcie bez dokumentów zależnych. |

### Stany - `/api/Inventory`

| Metoda i ścieżka | Dostęp | Działanie |
|---|---|---|
| `GET /api/Inventory` | `inventory.read` | Lista stanów w zakresie. |
| `GET /api/Inventory/{id}` | `inventory.read` | Szczegół. |
| `GET /api/Inventory/page-data` | `inventory.read` | Produkty i lokalizacje formularza. |
| `POST /api/Inventory` | `inventory.manage` | Stan produktu w lokalizacji. |
| `PUT /api/Inventory/{id}` | `inventory.manage` | Ilość i rezerwacja. |
| `DELETE /api/Inventory/{id}` | `inventory.manage` | Usunięcie stanu. |

Ilości mają `decimal(18,3)`, ceny `decimal(18,2)`. `Quantity >= 0`, `ReservedQuantity >= 0`, `ReservedQuantity <= Quantity`; para produkt-lokalizacja jest unikalna.

### Użytkownicy - `/api/Users`

| Metoda i ścieżka | Dostęp | Działanie |
|---|---|---|
| `GET /api/Users`, `GET /api/Users/{id}` | `users.read` | Lista i szczegóły bez hashy haseł. |
| `GET /api/Users/page-data` | `users.manage` | Role i magazyny formularza. |
| `POST /api/Users` | `users.manage` | Konto z loginem, hasłem, rolą i magazynem. |
| `PUT /api/Users/{id}` | `users.manage` | Edycja z ochroną administratorów. |
| `DELETE /api/Users/{id}` | `users.manage` | Usunięcie z ochroną własnego konta i administratorów. |

Login i e-mail są unikalne. Rola poza administratorem musi mieć `WarehouseId`.

### Role i uprawnienia

Wszystkie wymagają `roles.manage`.

| Metoda i ścieżka | Działanie |
|---|---|
| `GET/POST /api/Roles` | Lista albo utworzenie roli. |
| `GET/PUT/DELETE /api/Roles/{roleId}` | Szczegóły, edycja albo usunięcie roli. |
| `GET /api/Roles/permissions` | Katalog uprawnień. |
| `PUT /api/Roles/{roleId}/permissions` | Zastąpienie listy kodów roli. |
| `GET/POST /api/Permissions` | Lista albo utworzenie uprawnienia. |
| `GET/PUT/DELETE /api/Permissions/{id}` | Szczegóły, edycja albo usunięcie uprawnienia. |

Stałe role i uprawnienia systemowe są chronione przed usunięciem lub nieprawidłową zmianą.

### Dokumenty - `/api/StockDocuments`

| Metoda i ścieżka | Dostęp | Działanie/opcje |
|---|---|---|
| `GET /api/StockDocuments` | `stock-documents.read` | Lista; `type`, `page`, `pageSize`, `search`, `sortBy`, `order`. |
| `GET /api/StockDocuments/history` | `stock-documents.read` | Tylko `Completed`; opcjonalny typ, wyszukiwanie i sortowanie. |
| `GET /api/StockDocuments/{id}` | `stock-documents.read` | Szczegóły nagłówka i pozycji. |
| `GET /api/StockDocuments/page-data` | `stock-documents.manage` | Produkty, lokalizacje, magazyny i kontrahenci. |
| `GET /api/StockDocuments/shipment-page-data` | `stock-shipments.create` | Dane formularza WZ. |
| `GET /api/StockDocuments/scan/{id}` | `stock-documents.receive` | PZ do odbioru. |
| `POST /api/StockDocuments` | `stock-documents.manage` | Szkic PZ/WZ. |
| `POST /api/StockDocuments/shipments` | `stock-shipments.create` | Szkic WZ; typ wymuszony, kontrahent ignorowany. |
| `PUT /api/StockDocuments/{id}` | `stock-documents.manage` | Edycja wyłącznie szkicu. |
| `POST /api/StockDocuments/{id}/receive` | `stock-documents.receive` | Odbiór PZ, użytkownik i czas. |
| `POST /api/StockDocuments/{id}/complete` | `stock-documents.approve` | Zatwierdzenie i atomowa zmiana stanu. |
| `DELETE /api/StockDocuments/{id}` | `stock-documents.manage` | Usunięcie szkicu. |

Pozycja ma produkt, lokalizację i dodatnią ilość do trzech miejsc po przecinku. PZ wymaga dostawcy (`Supplier` lub `Both`), WZ innego magazynu docelowego. Lokalizacja musi należeć do magazynu dokumentu, a WZ nie może przekroczyć `AvailableQuantity`.

## 6. Frontend SPA

| Trasa | Funkcja |
|---|---|
| `/auth` | Logowanie. |
| `/main/management/products` | Produkty: wyszukiwanie, sortowanie, paginacja, CRUD. |
| `/main/management/categories` | Kategorie. |
| `/main/management/units` | Jednostki. |
| `/main/management/warehouses` | Magazyny. |
| `/main/management/locations` | Lokalizacje aktywnego magazynu. |
| `/main/management/inventory` | Stany i bezpośrednia edycja. |
| `/main/management/contractors` | Kontrahenci. |
| `/main/management/users` | Użytkownicy. |
| `/main/management/roles` | Role i uprawnienia. |
| `/main/shipments` | PZ/WZ, szkice, zatwierdzanie, historia. |
| `/main/account` | Własne konto i hasło. |

`authGuard` chroni `/main`, a `permissionGuard` ładuje użytkownika i uprawnienia. Brak uprawnienia kieruje do `/main/shipments`; API jest ostateczną ochroną. Listy mają wyszukiwanie, sortowanie, paginację, szczegóły, modale formularzy, potwierdzenia usuwania i komunikaty błędów.

## 7. Przypadki brzegowe

### Dane i zależności

- pusta baza bez magazynu uniemożliwia utworzenie zwykłego użytkownika;
- brak magazynu przypisanego użytkownikowi daje `403`;
- usunięcie zasobu z zależnościami kończy się konfliktem;
- SKU, login, e-mail, kod lokalizacji w magazynie, kod uprawnienia i nazwa roli są unikalne;
- ceny i ilości nie mogą być ujemne;
- rezerwacja nie może przekraczać ilości;
- produkt i lokalizacja muszą istnieć i należeć do właściwego zakresu;
- nie można dodać drugiego stanu dla produktu w lokalizacji;
- identyfikatory muszą być poprawnymi GUID-ami.

### Dokumenty

- dokument bez pozycji i pozycja z ilością `<= 0` są niepoprawne;
- PZ wymaga dostawcy, WZ wymaga innego magazynu docelowego;
- lokalizacja pozycji musi należeć do magazynu dokumentu;
- zatwierdzenie jest jednorazowe;
- odbiór PZ nie zastępuje zatwierdzenia;
- zatwierdzonego dokumentu nie można zmienić/usunąć;
- WZ z niewystarczającym stanem blokuje księgowanie;
- pełny `RowVersion` i osobny dziennik ruchów nie są jeszcze wdrożone.

### Bezpieczeństwo

- wygasły JWT jest odrzucany; tolerancja zegara wynosi minutę;
- zmiana hasła wymaga aktualnego hasła;
- administrator i role systemowe są chronione;
- klucz JWT, hasło SQL i `InitialAdmin` w `appsettings.json` są deweloperskie i muszą być zmienione poza lokalnym środowiskiem;
- CORS `AllowAnyOrigin` działa tylko w Development.

## 8. Przygotowanie magazynu od początku do końca

### A. Środowisko

1. Zainstaluj .NET SDK 10, Node.js/npm i SQL Server.
2. Ustaw connection string oraz `Jwt:SecretKey`, `Jwt:Issuer`, `Jwt:Audience` i czas wygaśnięcia.
3. Poza Development wyłącz lub zmień `InitialAdmin`, wymuś HTTPS i użyj silnego sekretu.
4. Uruchom API:

   ```bash
   dotnet run --project MagazineAPI/MagazineAPI/MagazineAPI.csproj
   ```

   Migracje wykonają się przy starcie. Można też użyć:

   ```bash
   dotnet ef database update \
     --project MagazineAPI/MagazineAPIInfrastructure/MagazineAPInfrastructure.csproj \
     --startup-project MagazineAPI/MagazineAPI/MagazineAPI.csproj
   ```

5. W Development sprawdź `/swagger` na `http://localhost:5292` lub `https://localhost:7000`.
6. Uruchom SPA:

   ```bash
   cd Magazine-SPA
   npm install
   npm start
   ```

   Sprawdź `src/environments/environment.development.ts`.

### B. Administrator

1. Przy pierwszym starcie `DatabaseInitializer` utworzy konto z `InitialAdmin`, jeśli jest włączony i nie istnieje administrator.
2. Zaloguj się tym kontem.
3. Zmień hasło przez `/main/account`.
4. Późniejsze zmiany pól `InitialAdmin` nie nadpisują istniejącego administratora.

### C. Struktura magazynu

1. Utwórz jednostki (`szt.`, `op.`, `kg`, `l`, `m`).
2. Utwórz kategorie.
3. Utwórz oddziały/magazyny z adresami.
4. Utwórz lokalizacje każdego magazynu: przyjęcia, regały, kompletacja, zwroty. Kod jest unikalny tylko w obrębie magazynu.
5. Utwórz kontrahentów z typem `Supplier`, `Customer` albo `Both`.

### D. Produkty

1. Po słownikach dodaj produkty.
2. Uzupełnij nazwę, SKU, opcjonalny barcode, kategorię, jednostkę, ceny i `IsActive`.
3. Produktu z zależnościami nie usuwaj; wycofuj go przez `IsActive`.

### E. Użytkownicy

1. Zostaw administratora bez magazynu.
2. Utwórz kierowników i przypisz ich do oddziałów.
3. Utwórz pracowników i przypisz ich do właściwych magazynów.
4. Skonfiguruj niestandardowe role przez `/main/management/roles`.
5. Przetestuj izolację danych między magazynami.

### F. Stan początkowy

- Pusty magazyn: utwórz stan z `Quantity=0`, a pierwszy towar wprowadź przez PZ.
- Istniejący magazyn: wprowadź stan przez `Inventory` użytkownikiem z `inventory.manage`.

Opcjonalnie po migracjach uruchom `MagazineAPI/scripts/seed-prosperous-multi-branch.sql`. Skrypt jest idempotentny i transakcyjny, ale ma dane demonstracyjne oraz hasło `Test123!`; nie używaj go w produkcji.

### G. Pierwsze PZ

1. Użytkownik z `stock-documents.manage` pobiera dane formularza.
2. Wybiera magazyn, dostawcę, produkty, lokalizacje i dodatnie ilości.
3. Zapisuje szkic; stan się nie zmienia.
4. Pracownik z `stock-documents.receive` skanuje GUID, sprawdza pozycje i odbiera PZ.
5. Kierownik z `stock-documents.approve` zatwierdza dokument.
6. Stan rośnie, dokument staje się `Completed`, a jego edycja i usunięcie są blokowane.

### H. Pierwsze WZ

1. Pracownik lub kierownik z `stock-shipments.create` wybiera magazyn źródłowy i inny docelowy.
2. Dodaje produkty i lokalizacje źródłowe w ilościach dostępnych.
3. Zapisuje szkic; stan się nie zmienia.
4. Kierownik zatwierdza WZ.
5. API sprawdza dostępność i transakcyjnie pomniejsza `Inventory` źródłowego magazynu.
6. Odbiór docelowy nie jest jeszcze osobnym procesem MM; pełny transfer jest w roadmapie.

### I. Kontrola

- zaloguj się jako administrator, kierownik i pracownik;
- sprawdź dane dwóch magazynów;
- wykonaj PZ od szkicu do zatwierdzenia i sprawdź wzrost stanu;
- wykonaj WZ z poprawną oraz zbyt dużą ilością;
- spróbuj usunąć rekord z zależnościami;
- sprawdź historię i błędne GUID-y;
- sprawdź `401` po wygaśnięciu tokenu.

## 9. Roadmapa

Poza aktualnym zakresem są: `StockMovement`, pełna historia ruchów, MM/PW/RW, transfer `Sent -> InTransit -> Received`, rezerwacje powiązane z dokumentami, inwentaryzacja, korekty, audit log, numeracja oddziałowa, dashboard, alerty minimum/optimum, CSV/PDF i sprzętowy czytnik kodów.

Najważniejsze ograniczenia produkcyjne: `Inventory` może być edytowany bezpośrednio, nie ma pełnego `RowVersion`, a księgowania nie mają osobnego audytu ruchów. Zalecana kolejność: ruchy magazynowe i atomowe księgowanie -> współbieżność -> kontrolowane korekty -> MM -> rezerwacje/inwentaryzacja/audyt -> numeracja i raporty -> dashboard SPA.

## 10. Weryfikacja projektu

Backend:

```bash
dotnet build MagazineAPI/MagazineAPI.slnx
dotnet test MagazineAPI/MagazineAPIApplication.Tests/MagazineAPIApplication.Tests.csproj
```

Frontend:

```bash
cd Magazine-SPA
npm run build
npm run lint
```

Przed wdrożeniem należy dodać testy integracyjne PZ/WZ, izolacji magazynów, współbieżnego księgowania, usuwania zależności oraz cyklu użytkownik -> uprawnienia -> endpoint.
