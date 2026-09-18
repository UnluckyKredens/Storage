# Audyt projektu Magazine v2 - 2026-10-03

## Zakres audytu

Audyt objął trzy części systemu:

- `MagazineAPI` - REST API ASP.NET Core, warstwa Application, Domain, Infrastructure, migracje EF Core.
- `Magazine-SPA` - aplikacja Angular, routing, strony modułów, serwisy HTTP i obsługa stanu widoków.
- `MagazineWarehouseBot` - bot symulujący pracę administratora, kierownika i pracowników magazynów.

Sprawdzone zostały kontrolery, endpointy, uprawnienia, główne handlery zapytań i komend, widoki SPA, miejsca inicjalizacji danych, ścieżki magazynowe PW/RW/MM, wysyłki, dashboard, historia ruchów, audit log oraz konfiguracja bazy.

## Wynik ogólny

Projekt kompiluje się i testy przechodzą po poprawkach wykonanych w audycie.

Wykonane weryfikacje:

- `npm run build` - OK.
- `npx ng test --watch=false` - OK, 24 testy.
- `dotnet build MagazineAPI/MagazineAPI.slnx --no-restore` - OK.
- `dotnet test MagazineAPI/MagazineAPI.slnx --no-restore` - OK, 7 testów.
- `dotnet build MagazineWarehouseBot/MagazineWarehouseBot.csproj --no-restore` - OK.
- `dotnet ef database update --context AppDbContext` - OK dla migracji optymalizującej wysyłki.

## Najważniejsze problemy znalezione w audycie

### 1. Frontend nie odświeżał widoków po odpowiedziach HTTP

Objaw: dashboard, wysyłki i dokumenty magazynowe potrafiły pokazywać loader albo pustą tabelę mimo tego, że API zwracało poprawny JSON.

Przyczyna: aplikacja była uruchomiona bez klasycznego `zone.js`, a wiele komponentów Angulara używało zwykłych pól klasowych aktualizowanych w `subscribe()`. W takim układzie odpowiedź HTTP nie zawsze wywołuje zmianę widoku.

Naprawa:

- dodano `zone.js` do zależności frontendu,
- dodano `polyfills: ["zone.js"]` w `Magazine-SPA/angular.json`,
- krytyczne widoki `Dashboard`, `Wysyłki` i `Dokumenty PW/RW/MM` przepisano dodatkowo na Angular signals tam, gdzie dane pochodzą z HTTP.

Status: naprawione globalnie oraz lokalnie w najbardziej problematycznych modułach.

### 2. Wysyłki odpalały za dużo requestów na starcie

Objaw: moduł wysyłek ładował się długo.

Przyczyna: ekran wysyłek wykonywał równocześnie pobranie wszystkich wysyłek, pobranie gotowych/w drodze oraz ciężkie `page-data`, mimo że część danych była potrzebna dopiero po kliknięciu formularza.

Naprawa:

- widok pobiera teraz tylko dane potrzebne dla aktualnej zakładki,
- `page-data` ładuje się dopiero po kliknięciu `Dodaj wysyłkę` lub `Prośba o wysyłkę`,
- dodano endpoint `GET /api/Shipments/pending`,
- dodano indeksy SQL dla wysyłek po statusie, magazynie i dacie.

Status: naprawione.

### 3. Dokumenty PW/RW/MM miały ten sam problem renderowania

Objaw: tabela dokumentów długo wyglądała jakby się ładowała.

Przyczyna: dane trafiały do zwykłej tablicy `operations`, a loader do zwykłego pola `loading`.

Naprawa:

- `operations`, `products`, `locations`, `loading`, `saving` zostały przeniesione na sygnały,
- komponent czeka na konto i uprawnienia przed pierwszym ładowaniem,
- opcje formularza są ładowane tylko dla użytkownika z prawem zarządzania.

Status: naprawione.

### 4. Część handlerów pobierała całe tabele do pamięci

Problem dotyczył głównie starszych prostych słowników oraz części komend. Najbardziej ryzykowne miejsca zostały już ograniczone w modułach dashboardu, wysyłek, dokumentów, ruchów magazynowych, produktów i stanów magazynowych.

Pozostałe miejsca do dalszej optymalizacji:

- `CreateShipmentRequestCommandHandler` nadal używa `AllAsync()` dla magazynów, lokalizacji i stanów,
- `CompleteWarehouseOperationCommandHandler` pobiera lokalizacje przez `AllAsync()`,
- proste słowniki nadal zwracają całe listy, co jest akceptowalne przy małych danych, ale wymaga paginacji przy większej skali.

Status: najcięższe widoki poprawione, pozostałe oznaczone jako ograniczenia.

### 5. Brak pełnej automatyzacji testów end-to-end

Projekt ma testy jednostkowe, ale nie ma kompletnego zestawu testów E2E, który sprawdzałby realne przejście przez UI: logowanie, wybór magazynu, dashboard, dokumenty, wysyłki, odbiór wysyłki i raporty.

Status: ryzyko projektowe. Zalecane Playwright/Cypress.

### 6. Audyt npm wykazał podatności w zależnościach

Po instalacji zależności `npm audit` zgłosił 47 podatności: 11 moderate, 34 high, 2 critical.

Nie zastosowano automatycznego `npm audit fix --force`, bo może wprowadzić breaking changes. Trzeba przejrzeć podatności osobno przed oddaniem/produkcyjnym wdrożeniem.

Status: do obsługi przed produkcją.

## Sprawdzone moduły funkcjonalne

### Uwierzytelnianie i konto

Endpointy:

- `POST /api/Auth/login`
- `GET /api/Auth/IsAuthenticated`
- `GET /api/Auth/me`
- `PUT /api/Auth/me`
- `GET /api/Auth/me/permissions`

Wnioski:

- logowanie używa JWT,
- konto użytkownika i uprawnienia są pobierane przez `AccountService`,
- role i uprawnienia sterują routingiem oraz widocznością modułów,
- administrator ma specjalny bypass uprawnień.

Ryzyka:

- konto i uprawnienia są ładowane w wielu komponentach niezależnie; warto docelowo scentralizować inicjalizację w layoucie lub resolverze.

### Dashboard

Endpoint:

- `GET /api/WarehouseDashboard`

Funkcje:

- liczba produktów,
- pozycje magazynowe,
- ilości całkowite, zarezerwowane i dostępne,
- liczba aktywnych rezerwacji,
- liczba wysyłek oczekujących, wysłanych i w drodze,
- alerty niskiego stanu.

Wnioski:

- endpoint działał poprawnie, problem był po stronie renderowania SPA,
- komponent został przepisany na signals,
- dashboard administratora uwzględnia wybrany aktywny magazyn.

Status: naprawione.

### Produkty

Endpointy:

- `GET /api/Products`
- `GET /api/Products/{id}`
- `GET /api/Products/page-data`
- `POST /api/Products`
- `PUT /api/Products/{id}`
- `DELETE /api/Products/{id}`

Funkcje:

- lista produktów z paginacją, sortowaniem i wyszukiwaniem,
- obsługa SKU, kodu kreskowego, kategorii, jednostki, ceny i aktywności,
- formularz tworzenia i edycji.

Wnioski:

- lista produktów ma serwerową paginację,
- endpoint `page-data` pobiera kategorie i jednostki do formularza,
- jest unikalność SKU.

Ryzyka:

- warto dodać testy walidacji kodów kreskowych i unikalności przy edycji.

### Stany magazynowe

Endpointy:

- `GET /api/Inventory`
- `GET /api/Inventory/{id}`
- `GET /api/Inventory/page-data`
- `POST /api/Inventory`
- `PUT /api/Inventory/{id}`
- `DELETE /api/Inventory/{id}`

Funkcje:

- stan per produkt i lokalizacja,
- ilość całkowita, zarezerwowana i dostępna,
- filtrowanie po aktywnym magazynie,
- formularz dodania/edycji stanu.

Wnioski:

- widok korzysta z filtrowania po kontekście magazynu,
- dostępność jest powiązana z rezerwacjami.

Ryzyka:

- przy dużej liczbie pozycji warto dodać paginację serwerową podobną do produktów.

### Dokumenty magazynowe PW/RW/MM

Endpointy:

- `GET /api/WarehouseOperations`
- `POST /api/WarehouseOperations/complete`

Funkcje:

- dokument PW,
- dokument RW,
- przesunięcie MM,
- korekta,
- inwentaryzacja,
- generowanie numerów dokumentów,
- zapis ruchów magazynowych,
- wpływ na stany i historię.

Wnioski:

- moduł działa w oparciu o `WarehouseOperation`,
- lista ogranicza wynik do ostatnich 500 dokumentów,
- dodany jest indeks `WarehouseId + CompletedOnUtc`,
- frontend został poprawiony na signals.

Status: naprawione dla problemu ładowania.

### Ruchy magazynowe

Endpoint:

- `GET /api/StockMovements`

Funkcje:

- lista zmian stanu,
- filtrowanie po magazynie, produkcie i źródle ruchu,
- limit wyników.

Wnioski:

- endpoint ma limit i indeksy po dacie, magazynie, produkcie oraz źródle,
- widok jest raportowy.

Ryzyka:

- przy większym wolumenie lepsza będzie paginacja zamiast samego limitu.

### Wysyłki

Endpointy:

- `GET /api/Shipments`
- `GET /api/Shipments/ready`
- `GET /api/Shipments/pending`
- `GET /api/Shipments/{id}`
- `GET /api/Shipments/lookup`
- `GET /api/Shipments/{id}/history`
- `GET /api/Shipments/page-data`
- `GET /api/Shipments/product-by-barcode`
- `GET /api/Shipments/product`
- `POST /api/Shipments`
- `POST /api/Shipments/request`
- `POST /api/Shipments/{id}/approve`
- `POST /api/Shipments/{id}/in-transit`
- `POST /api/Shipments/{id}/receive`

Funkcje:

- tworzenie wysyłki,
- prośba o wysyłkę,
- akceptacja,
- status `Wysłana`,
- status `W drodze`,
- odbiór i przyjęcie na stan,
- historia wysyłki,
- wydruk WZ i etykiety,
- wyszukiwanie po identyfikatorze,
- kody QR i kreskowe.

Wnioski:

- moduł jest najbardziej złożony i wymaga testów E2E,
- dodano endpoint `pending`,
- ograniczono liczbę requestów na starcie,
- dodano indeksy do wyszukiwania po statusie i magazynach.

Status: poprawione ładowanie i wydajność widoku.

### Magazyny i lokalizacje

Endpointy:

- `GET /api/Warehouses`
- `GET /api/Warehouses/{id}`
- `POST /api/Warehouses`
- `PUT /api/Warehouses/{id}`
- `DELETE /api/Warehouses/{id}`
- `GET /api/Locations`
- `GET /api/Locations/{id}`
- `GET /api/Locations/page-data`
- `POST /api/Locations`
- `PUT /api/Locations/{id}`
- `DELETE /api/Locations/{id}`

Funkcje:

- oddziały/magazyny,
- lokalizacje w magazynach,
- aktywny magazyn w nagłówku,
- filtrowanie danych po magazynie dla użytkowników nieadministracyjnych.

Wnioski:

- unikalność lokalizacji jest per magazyn,
- administrator może przełączać magazyn.

Ryzyka:

- warto dopisać test E2E przełączania magazynu i sprawdzenia, czy tabele zmieniają zakres danych.

### Użytkownicy, role, uprawnienia

Endpointy:

- `GET /api/Users`
- `GET /api/Users/{id}`
- `GET /api/Users/page-data`
- `POST /api/Users`
- `PUT /api/Users/{id}`
- `DELETE /api/Users/{id}`
- `GET /api/Roles`
- `GET /api/Roles/{id}`
- `POST /api/Roles`
- `PUT /api/Roles/{id}`
- `DELETE /api/Roles/{id}`
- `GET /api/Roles/permissions`
- `PUT /api/Roles/{id}/permissions`
- `GET /api/Permissions`
- `GET /api/Permissions/{id}`
- `POST /api/Permissions`
- `PUT /api/Permissions/{id}`
- `DELETE /api/Permissions/{id}`

Funkcje:

- administrator,
- kierownik,
- pracownik,
- przypisanie pracownika do magazynu,
- role i katalog uprawnień,
- zarządzanie dostępem do modułów.

Wnioski:

- model RBAC jest czytelny,
- administrator ma dostęp globalny,
- role pracowników są powiązane z magazynami.

Ryzyka:

- brakuje większej liczby testów scenariuszy uprawnień.

### Kontrahenci, kategorie, jednostki

Endpointy:

- `GET/POST/PUT/DELETE /api/Contractors`
- `GET/POST/PUT/DELETE /api/Category`
- `GET/POST/PUT/DELETE /api/UnitsOfMeasure`

Funkcje:

- słowniki pomocnicze,
- dostawcy/kontrahenci,
- kategorie i jednostki produktów.

Wnioski:

- przy obecnej skali proste listy są akceptowalne,
- przy większej skali warto dodać paginację.

### Audit log

Endpoint:

- `GET /api/AuditLogs`

Funkcje:

- historia kluczowych operacji,
- filtrowanie po encji, użytkowniku i limicie.

Wnioski:

- logi są ograniczane limitem,
- dostęp tylko dla uprawnień administracyjnych.

Ryzyka:

- warto dodać filtr zakresu dat w UI.

### WarehouseBot

Zakres:

- administrator,
- kierownik,
- dwóch pracowników na oddział,
- samodzielna obsługa bazy przez API,
- tworzenie danych startowych,
- akcje magazynowe,
- wysyłki i odbiory.

Wnioski:

- bot kompiluje się,
- konfiguracja wspiera pracę wielu ról,
- bot korzysta z API zamiast bezpośrednio modyfikować bazę.

Ryzyka:

- brakuje testu integracyjnego bota z żywym API i bazą testową.

## Poprawki wykonane w tym audycie

- Naprawiono dashboard magazynu przez signals.
- Naprawiono wysyłki przez ograniczenie requestów startowych, endpoint `pending`, signals i indeksy SQL.
- Naprawiono dokumenty PW/RW/MM przez signals i uporządkowanie inicjalizacji.
- Dodano `zone.js` i polyfills, żeby wszystkie klasyczne komponenty Angulara odświeżały UI po HTTP.
- Dodano migrację `20261003093616_OptimizeShipmentQueries`.
- Zaktualizowano lokalną bazę migracją optymalizującą wysyłki.

## Pozostałe zalecenia techniczne

1. Dodać testy E2E dla pełnej ścieżki: login, wybór magazynu, dashboard, dokument PW/RW/MM, wysyłka, akceptacja, status w drodze, odbiór.
2. Ujednolicić frontend: docelowo wszystkie większe widoki tabelaryczne przenieść na signals albo scentralizowany store.
3. Dodać paginację serwerową dla stanów magazynowych, dokumentów, ruchów i audit logu.
4. Ograniczyć pozostałe `AllAsync()` w komendach ciężkich biznesowo.
5. Przejrzeć `npm audit` i świadomie zaktualizować zależności.
6. Dodać testy uprawnień dla ról administrator/kierownik/pracownik.
7. Dodać scenariusz testowy WarehouseBota uruchamiany przeciwko bazie testowej.

## Konkluzja

Projekt ma sensowną architekturę warstwową i szeroki zakres funkcjonalny dla systemu magazynowego. Największy praktyczny problem nie leżał w pojedynczym endpointcie, tylko w sposobie odświeżania UI i w nadmiarowym ładowaniu danych na starcie modułów. Po poprawkach dashboard, wysyłki i dokumenty są naprawione, a globalne włączenie `zone.js` stabilizuje pozostałe klasyczne komponenty Angulara.

