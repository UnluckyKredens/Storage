# Audyt projektu Magazine v2

Data audytu: 2026-10-02  
Zakres: `MagazineAPI`, `Magazine-SPA`, `MagazineWarehouseBot`, dokumentacja i konfiguracja repozytorium.

## 1. Podsumowanie

Projekt jest kompletną aplikacją magazynową typu full-stack. Backend działa w ASP.NET Core/.NET 10, frontend w Angular 21, a osobny `MagazineWarehouseBot` symuluje pracę użytkowników przez publiczne endpointy API. Architektura jest czytelnie podzielona na warstwy: HTTP API, Application/CQRS, Domain oraz Infrastructure/EF Core.

Weryfikacja techniczna zakończyła się pozytywnie:

| Komenda | Wynik |
|---|---|
| `dotnet build MagazineAPI/MagazineAPI.slnx --no-restore` | OK, 0 błędów, 0 ostrzeżeń |
| `dotnet test MagazineAPI/MagazineAPI.slnx --no-restore` | OK, 7/7 testów |
| `dotnet build MagazineWarehouseBot/MagazineWarehouseBot.csproj --no-restore` | OK, 0 błędów, 0 ostrzeżeń |
| `npm run build` w `Magazine-SPA` | OK |
| `npm run lint` w `Magazine-SPA` | OK |
| `npx ng test --watch=false` w `Magazine-SPA` | OK, 24/24 testów |

Najważniejsze ryzyka do poprawy przed wersją produkcyjną:

1. W `MagazineAPI/MagazineAPI/appsettings.json` znajdują się jawne sekrety deweloperskie: JWT secret, hasło SQL i hasło początkowego administratora.
2. Bot ma rozjazd z aktualnym workflow wysyłek: próbuje odbierać wysyłkę bez body checklisty i szuka statusu, którego API nie zwraca.
3. Krytyczne procesy magazynowe mają za mało testów automatycznych: rezerwacje, zatwierdzanie wysyłek, odbiór, PW/RW/MM, korekty i audyt.
4. Część zapytań ładuje pełne tabele do pamięci i filtruje w aplikacji, co będzie problemem przy większej bazie.
5. Nie ma gotowego testu end-to-end z prawdziwą bazą SQL Server i scenariuszem użytkownika przez SPA.

## 2. Architektura

### Backend

- `MagazineAPI` - kontrolery HTTP, JWT, Swagger, CORS dev, obsługa wyjątków, rejestracja DI.
- `MagazineAPIApplication` - komendy i zapytania Mediator/CQRS, walidacja biznesowa, modele odczytu.
- `MagazineAPIDomain` - encje, enumy, role, kody uprawnień i interfejsy repozytoriów.
- `MagazineAPIInfrastructure` - EF Core, migracje, konfiguracje encji, repozytoria, `UnitOfWork`.
- `MagazineAPIEvent` - zdarzenia i handlery zapisujące historię.

Wzorzec jest dobry dla pracy inżynierskiej: separacja warstw jest widoczna i można ją łatwo opisać na diagramach.

### Frontend

- Angular 21 z lazy-loaded routes.
- `auth` - logowanie, konto, token, interceptory, guardy.
- `management` - CRUD i widoki operacyjne.
- `dashboard` - pulpit magazynowy.
- Angular Material, RxJS, NgRx Signals.

Frontend konsekwentnie używa osobnych usług HTTP dla modułów i guardów uprawnień.

### Bot

`MagazineWarehouseBot` ma własny panel webowy na `http://localhost:5260`, loguje użytkowników do API i wykonuje akcje przez HTTP. To dobry element demonstracyjny, ale obecnie wymaga dopasowania do aktualnego workflow wysyłek.

## 3. Endpointy API

Wszystkie endpointy poza logowaniem są zabezpieczone JWT albo polityką `HasPermission`.

### Auth

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `POST /api/Auth/login` | publiczny | Logowanie i zwrot JWT | OK |
| `GET /api/Auth/IsAuthenticated` | JWT | Sprawdzenie ważności tokenu | OK |
| `GET /api/Auth/me` | JWT | Dane zalogowanego użytkownika | OK |
| `PUT /api/Auth/me` | JWT | Edycja własnego konta i zmiana hasła | OK |
| `GET /api/Auth/me/permissions` | JWT | Efektywne kody uprawnień | OK |

### Produkty

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/Products` | `products.read` | Lista, paginacja, wyszukiwanie, sortowanie | OK; filtrowanie po stronie aplikacji |
| `GET /api/Products/{id}` | `products.read` | Szczegóły produktu | OK |
| `GET /api/Products/page-data` | `products.read` | Kategorie i jednostki do formularza | OK |
| `POST /api/Products` | `products.manage` | Utworzenie produktu | OK |
| `PUT /api/Products/{id}` | `products.manage` | Edycja produktu | OK |
| `DELETE /api/Products/{id}` | `products.manage` | Usunięcie produktu | OK; zależności dają konflikt |

### Kategorie

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/Category` | `dictionaries.manage` | Lista kategorii | OK |
| `GET /api/Category/{id}` | `dictionaries.manage` | Szczegóły kategorii | OK |
| `POST /api/Category` | `dictionaries.manage` | Utworzenie kategorii | OK |
| `PUT /api/Category/{id}` | `dictionaries.manage` | Edycja kategorii | OK |
| `DELETE /api/Category/{id}` | `dictionaries.manage` | Usunięcie kategorii | OK |

Uwaga: brak osobnego uprawnienia tylko do odczytu słowników.

### Jednostki miary

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/UnitsOfMeasure` | `dictionaries.manage` | Lista jednostek | OK |
| `GET /api/UnitsOfMeasure/{id}` | `dictionaries.manage` | Szczegóły jednostki | OK |
| `POST /api/UnitsOfMeasure` | `dictionaries.manage` | Utworzenie jednostki | OK |
| `PUT /api/UnitsOfMeasure/{id}` | `dictionaries.manage` | Edycja jednostki | OK |
| `DELETE /api/UnitsOfMeasure/{id}` | `dictionaries.manage` | Usunięcie jednostki | OK |

### Magazyny

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/Warehouses` | `warehouses.read` | Lista magazynów | OK |
| `GET /api/Warehouses/{id}` | `warehouses.read` | Szczegóły magazynu | OK |
| `POST /api/Warehouses` | `warehouses.manage` | Utworzenie magazynu | OK |
| `PUT /api/Warehouses/{id}` | `warehouses.manage` | Edycja magazynu | OK |
| `DELETE /api/Warehouses/{id}` | `warehouses.manage` | Usunięcie magazynu | OK |

### Lokalizacje

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/Locations` | `warehouses.read` | Lista lokalizacji | OK |
| `GET /api/Locations/{id}` | `warehouses.read` | Szczegóły lokalizacji | OK |
| `GET /api/Locations/page-data` | `warehouses.read` | Magazyny do formularza | OK |
| `POST /api/Locations` | `warehouses.manage` | Utworzenie lokalizacji | OK |
| `PUT /api/Locations/{id}` | `warehouses.manage` | Edycja lokalizacji | OK |
| `DELETE /api/Locations/{id}` | `warehouses.manage` | Usunięcie lokalizacji | OK |

### Stany magazynowe

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/Inventory` | `inventory.read` | Lista stanów | OK |
| `GET /api/Inventory/{id}` | `inventory.read` | Szczegóły stanu | OK |
| `GET /api/Inventory/page-data` | `inventory.read` | Produkty i lokalizacje do formularza | OK |
| `POST /api/Inventory` | `inventory.manage` | Utworzenie stanu | OK |
| `PUT /api/Inventory/{id}` | `inventory.manage` | Edycja ilości i rezerwacji | OK |
| `DELETE /api/Inventory/{id}` | `inventory.manage` | Usunięcie stanu | OK |

Każda zmiana zapisywana jest jako `StockMovement` i częściowo jako `AuditLog`.

### Ruchy magazynowe

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/StockMovements` | `inventory.read` | Historia ruchów z filtrami `productId`, `warehouseId`, `sourceId`, `limit` | OK; filtracja po pobraniu pełnej listy |

### Operacje magazynowe

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/WarehouseOperations` | `inventory.read` | Lista dokumentów PW/RW/MM/korekt/inwentaryzacji | OK |
| `POST /api/WarehouseOperations/complete` | `inventory.manage` | Księgowanie operacji magazynowej | OK; wymaga testów transakcyjnych |

Obsługiwane typy: `InternalReceipt`, `InternalIssue`, `InternalTransfer`, `Correction`, `InventoryCount`.

### Dashboard

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/WarehouseDashboard` | `inventory.read` | KPI, alerty i liczniki wysyłek | OK; zapytania agregują w pamięci |

### Wysyłki

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/Shipments` | `shipments.read` | Lista wysyłek w zakresie magazynu | OK |
| `GET /api/Shipments/ready` | `shipments.read` | Wysłane i w drodze | OK |
| `GET /api/Shipments/{id}` | `shipments.read` | Szczegóły wysyłki | OK |
| `GET /api/Shipments/lookup?identifier=` | `shipments.approve` | Wyszukanie po GUID lub numerze | OK |
| `GET /api/Shipments/{id}/history` | `shipments.read` | Historia zdarzeń wysyłki | OK |
| `GET /api/Shipments/page-data` | `shipments.create` | Dane do tworzenia wysyłki | OK |
| `GET /api/Shipments/product-by-barcode` | `shipments.create` | Produkt po kodzie kreskowym | OK |
| `GET /api/Shipments/product` | `shipments.create` | Produkt po ID | OK |
| `POST /api/Shipments` | `shipments.create` | Utworzenie wysyłki z magazynu źródłowego | OK |
| `POST /api/Shipments/request` | `shipments.create` | Prośba o wysyłkę z automatycznym doborem źródła | OK |
| `POST /api/Shipments/{id}/approve` | `shipments.approve` | Zużycie rezerwacji i status `Sent` | OK |
| `POST /api/Shipments/{id}/in-transit` | `shipments.approve` | Status `InTransit` | OK |
| `POST /api/Shipments/{id}/receive` | `shipments.approve` | Przyjęcie do magazynu docelowego z checklistą | OK w SPA; bot niezgodny |

### Kontrahenci

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/Contractors` | `contractors.read` | Lista kontrahentów | OK |
| `GET /api/Contractors/{id}` | `contractors.read` | Szczegóły kontrahenta | OK |
| `POST /api/Contractors` | `contractors.manage` | Utworzenie kontrahenta | OK |
| `PUT /api/Contractors/{id}` | `contractors.manage` | Edycja kontrahenta | OK |
| `DELETE /api/Contractors/{id}` | `contractors.manage` | Usunięcie kontrahenta | OK |

### Użytkownicy

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/Users` | `users.read` | Lista użytkowników | OK |
| `GET /api/Users/{id}` | `users.read` | Szczegóły użytkownika | OK |
| `GET /api/Users/page-data` | `users.manage` | Role i magazyny do formularza | OK |
| `POST /api/Users` | `users.manage` | Utworzenie konta | OK |
| `PUT /api/Users/{id}` | `users.manage` | Edycja konta, roli, magazynu i hasła | OK |
| `DELETE /api/Users/{id}` | `users.manage` | Usunięcie konta | OK |

### Role

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/Roles` | `roles.manage` | Lista ról z uprawnieniami | OK |
| `GET /api/Roles/{roleId}` | `roles.manage` | Szczegóły roli | OK |
| `POST /api/Roles` | `roles.manage` | Utworzenie roli | OK |
| `PUT /api/Roles/{roleId}` | `roles.manage` | Edycja roli | OK |
| `DELETE /api/Roles/{roleId}` | `roles.manage` | Usunięcie roli | OK |
| `GET /api/Roles/permissions` | `roles.manage` | Katalog uprawnień | OK |
| `PUT /api/Roles/{roleId}/permissions` | `roles.manage` | Zastąpienie uprawnień roli | OK |

### Uprawnienia

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/Permissions` | `roles.manage` | Lista uprawnień | OK |
| `GET /api/Permissions/{id}` | `roles.manage` | Szczegóły uprawnienia | OK |
| `POST /api/Permissions` | `roles.manage` | Utworzenie uprawnienia | OK |
| `PUT /api/Permissions/{id}` | `roles.manage` | Edycja uprawnienia | OK |
| `DELETE /api/Permissions/{id}` | `roles.manage` | Usunięcie uprawnienia | OK |

### Audit log

| Endpoint | Dostęp | Funkcja | Status |
|---|---|---|---|
| `GET /api/AuditLogs` | `roles.manage` | Lista logów z filtrami `entityName`, `entityId`, `limit` | OK; brak osobnego uprawnienia audytowego |

## 4. Funkcjonalności SPA

| Trasa | Funkcjonalność | Status |
|---|---|---|
| `/auth` | Logowanie | OK |
| `/main/dashboard` | Pulpit KPI, alerty, wysyłki | OK |
| `/main/management` | Katalog modułów zależny od uprawnień | OK |
| `/main/management/products` | Produkty: CRUD, paginacja, wyszukiwanie | OK |
| `/main/management/categories` | Kategorie | OK |
| `/main/management/units` | Jednostki miary | OK |
| `/main/management/warehouses` | Magazyny | OK |
| `/main/management/locations` | Lokalizacje | OK |
| `/main/management/inventory` | Stany magazynowe | OK |
| `/main/management/warehouse-operations` | PW/RW/MM/korekta/inwentaryzacja | OK |
| `/main/management/stock-movements` | Historia ruchów | OK |
| `/main/management/shipments` | Menu wysyłek | OK |
| `/main/management/shipments/all` | Wszystkie wysyłki | OK |
| `/main/management/shipments/pending` | Wysyłki oczekujące | OK |
| `/main/management/shipments/ready` | Wysłane i w drodze | OK |
| `/main/management/contractors` | Kontrahenci | OK |
| `/main/management/users` | Użytkownicy | OK |
| `/main/management/roles` | Role i przypisania uprawnień | OK |
| `/main/management/permissions` | Katalog uprawnień | OK |
| `/main/management/audit-logs` | Audit log | OK |
| `/main/account` | Profil, dane konta, zmiana hasła | OK |

Uwaga UX: linki główne w navbarze (`Dashboard`, `Magazyn`, `Wysyłki`, `Zarządzanie`) są widoczne niezależnie od uprawnień. Route guard zabezpiecza dostęp, ale UX byłby czytelniejszy, gdyby navbar też ukrywał niedostępne obszary.

## 5. Funkcjonalności backendu

Zrealizowane:

- JWT, haszowanie haseł PBKDF2/SHA512.
- Role i uprawnienia z administracyjnym bypass.
- Izolacja magazynu przez `IWarehouseContext` i `X-Warehouse-Id`.
- CRUD dla produktów, kategorii, jednostek, magazynów, lokalizacji, kontrahentów, użytkowników, ról i uprawnień.
- Stany magazynowe z ilością całkowitą, rezerwacją i kolumną wyliczaną `AvailableQuantity`.
- Historia ruchów `StockMovement`.
- Audit log dla kluczowych decyzji.
- Numeracja dokumentów per magazyn, typ i rok.
- Rezerwacje towaru dla wysyłek.
- Workflow wysyłki: `PendingApproval -> Sent -> InTransit -> Received`.
- Operacje PW/RW/MM/korekta/inwentaryzacja.
- Dashboard z KPI i alertami minimum/optimum.
- Migracje EF Core i seed ról/uprawnień.

## 6. Funkcjonalności bota

Zrealizowane:

- Panel webowy z konfiguracją kont.
- Logowanie kont przez API.
- Harmonogram akcji per aktor.
- Tworzenie produktów i stanów.
- Edycja stanów.
- Tworzenie wysyłek i próśb o wysyłkę.
- Zatwierdzanie wysyłek.
- Czyszczenie rekordów wygenerowanych przez bota.

Problem:

- Odbiór wysyłek w bocie jest niezgodny z API. Bot szuka statusu `Gotowa`, a API zwraca `Wysłana`, `W drodze`, `Odebrana`. API wymaga statusu `W drodze` i body `{ checkedItemIds, notes }`, natomiast bot wywołuje `POST /api/Shipments/{id}/receive` bez body i bez przejścia przez `POST /api/Shipments/{id}/in-transit`.

## 7. Znalezione problemy i rekomendacje

### Wysoki priorytet

1. Jawne sekrety w repozytorium  
   Plik: `MagazineAPI/MagazineAPI/appsettings.json`  
   Ryzyko: przejęcie bazy, JWT i konta administratora po przeniesieniu konfiguracji poza lokalne demo.  
   Rekomendacja: przenieść `Jwt:SecretKey`, connection stringi i `InitialAdmin` do User Secrets, zmiennych środowiskowych albo sejfu sekretów. W repo zostawić tylko `appsettings.example.json`.

2. Bot nie odbiera wysyłek zgodnie z API  
   Pliki: `MagazineWarehouseBot/Api/MagazineApiClient.cs`, `MagazineWarehouseBot/Simulation/WarehouseBotService.cs`, `MagazineAPI/MagazineAPI/Controllers/ShipmentsController.cs`  
   Rekomendacja: dodać metodę `MarkShipmentInTransitAsync`, filtrować status `Wysłana` dla przejścia w drogę, potem `W drodze` dla odbioru, pobierać pozycje i wysyłać body z `checkedItemIds`.

3. Brak testów krytycznych procesów magazynowych  
   Obecne testy backendu obejmują głównie użytkowników i konto własne.  
   Rekomendacja: dopisać testy handlerów: `CreateShipment`, `ApproveShipment`, `MarkShipmentInTransit`, `ReceiveShipment`, `CompleteWarehouseOperation`, `SaveInventory`, `GetWarehouseDashboard`.

### Średni priorytet

4. Zapytania filtrują duże zbiory w pamięci  
   Przykłady: produkty, wysyłki, dashboard, ruchy magazynowe, audit log.  
   Rekomendacja: dla dużych tabel przenieść filtrowanie, sortowanie i paginację do zapytań EF/IQueryable albo dedykowanych repozytoriów read-only.

5. Operacje z wieloma zapisami są częściowo rozproszone  
   `Repository` wykonuje `SaveChangesAsync` w każdej metodzie. `UnitOfWork` trzyma transakcję, ale pojedyncze handlery nadal mieszają zapis encji, ruchów, audytu i publikację zdarzeń.  
   Rekomendacja: dla krytycznych operacji użyć jednego spójnego wzorca transakcyjnego oraz rozważyć outbox dla zdarzeń.

6. Audit log nie zawsze jest w tej samej transakcji co decyzja biznesowa  
   Przykładowo zatwierdzenie i odbiór wysyłki zapisują część audytu po transakcji.  
   Rekomendacja: log decyzji zapisywać w tej samej transakcji albo świadomie opisać to jako kompromis.

7. Brak osobnych uprawnień odczytu słowników i audytu  
   `Category` i `UnitsOfMeasure` wymagają `dictionaries.manage`, a `AuditLogs` używa `roles.manage`.  
   Rekomendacja: rozważyć `dictionaries.read` i `audit.read`, jeśli system ma być precyzyjniej sterowany rolami.

8. Brak e2e/integrowanej walidacji z SQL Server  
   Rekomendacja: dodać Docker Compose dla SQL Server i test smoke: migracja, logowanie admina, utworzenie magazynu, produktu, stanu, wysyłki i odbioru.

### Niski priorytet

9. Nieosiągalny kod w handlerze operacji magazynowych  
   Plik: `CompleteWarehouseOperationCommandHandler.cs`  
   W metodzie `GetOrCreateInventoryAsync` występuje podwójne `return inventory;`.  
   Rekomendacja: usunąć drugi `return`.

10. Dokumentacja częściowo się zestarzała  
   Część README opisuje funkcje jako roadmapę, choć są już zaimplementowane.  
   Rekomendacja: zaktualizować README po audycie i wskazać jeden dokument jako źródło prawdy.

## 8. Bezpieczeństwo

Mocne strony:

- JWT Bearer i walidacja issuer/audience/lifetime.
- PBKDF2 z SHA512 i stałoczasowe porównanie hasła.
- Uprawnienia kontrolowane po stronie API.
- Administrator rozpoznawany po stałym ID roli.
- Zwykły użytkownik pracuje tylko w przypisanym magazynie.
- Globalny exception handler nie zwraca surowych błędów serwera.

Ryzyka:

- Sekrety w repozytorium.
- `InitialAdmin.Enabled = true` w domyślnej konfiguracji.
- Automatyczne migracje przy starcie API są wygodne lokalnie, ale ryzykowne produkcyjnie.
- Brak rate limitingu logowania.
- Brak refresh tokenów i mechanizmu unieważniania tokenu po zmianie uprawnień.

## 9. Baza danych

Mocne strony:

- GUID jako identyfikatory.
- Konfiguracje EF Core oddzielone per encja.
- Indeksy i ograniczenia unikalności dla kluczowych pól.
- Check constrainty dla ilości, cen, statusów, magazynów wysyłki i przypisania magazynu użytkownika.
- `AvailableQuantity` jako kolumna wyliczana.

Ryzyka:

- Brak `RowVersion`/optymistycznej kontroli współbieżności dla `Inventory`.
- Operacje rezerwacji i ruchów powinny być przetestowane przy równoległych żądaniach.
- Brak archiwizacji/retencji audit logów i ruchów.

## 10. Testy

Backend:

- 7 testów, wszystkie przechodzą.
- Zakres: głównie tworzenie użytkownika i edycja własnego konta.
- Brakuje testów integracyjnych API i bazy.

Frontend:

- 24 testy, wszystkie przechodzą.
- Zakres: routing, guardy, konto, layout.
- Brakuje testów stron operacyjnych i usług dla wysyłek/operacji magazynowych.

Bot:

- Build przechodzi.
- Brak testów automatycznych.

## 11. Wnioski końcowe

Projekt jest dobrym materiałem na pracę inżynierską: ma realny problem biznesowy, sensowną architekturę, autoryzację, izolację danych magazynowych, operacje magazynowe, historię ruchów, audit log, dashboard i symulator. Do obrony warto szczególnie podkreślić przejście od prostego CRUD do systemu procesowego: rezerwacje, workflow wysyłek, dokumenty PW/RW/MM, numerację dokumentów i śledzenie zmian.

Przed pokazem końcowym najlepiej poprawić bota, usunąć jawne sekrety z domyślnej konfiguracji i dodać kilka testów procesów magazynowych. To podniesie wiarygodność projektu bardziej niż kolejne funkcje UI.
