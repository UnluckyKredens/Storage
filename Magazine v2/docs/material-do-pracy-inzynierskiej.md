# Co uwzględnić w pracy inżynierskiej o projekcie Magazine v2

Ten dokument jest szkieletem treści do pracy inżynierskiej. Można go potraktować jako checklistę rozdziałów, diagramów, opisów technicznych i argumentów, które warto pokazać promotorowi/recenzentowi.

## 1. Proponowany tytuł

**Projekt i implementacja webowego systemu wspomagającego obsługę procesów magazynowych w przedsiębiorstwie wielooddziałowym**

Alternatywy:

- **System zarządzania stanami magazynowymi z kontrolą uprawnień i obsługą wysyłek między magazynami**
- **Aplikacja full-stack do ewidencji stanów, rezerwacji i operacji magazynowych**

## 2. Cel pracy

Główny cel:

> Celem pracy jest zaprojektowanie i implementacja aplikacji webowej wspierającej zarządzanie produktami, magazynami, lokalizacjami, stanami magazynowymi, wysyłkami oraz uprawnieniami użytkowników w przedsiębiorstwie posiadającym wiele oddziałów.

Cele szczegółowe:

- opracowanie modelu danych dla produktów, magazynów, lokalizacji, użytkowników, ról i operacji magazynowych;
- implementacja API REST w architekturze warstwowej;
- implementacja SPA dla pracowników, kierowników i administratorów;
- wdrożenie uwierzytelniania JWT i autoryzacji opartej o role/uprawnienia;
- zapewnienie izolacji danych według magazynu;
- obsługa stanów całkowitych, zarezerwowanych i dostępnych;
- obsługa wysyłek między magazynami z rezerwacją, akceptacją, transportem i odbiorem;
- zapis historii ruchów magazynowych i audit logu;
- przygotowanie danych demonstracyjnych oraz symulatora aktywności użytkowników.

## 3. Problem biznesowy

Opisz, że firma wielooddziałowa potrzebuje jednego systemu do:

- kontroli bieżących stanów magazynowych;
- rozdzielenia odpowiedzialności między pracowników, kierowników i administratorów;
- ograniczenia widoczności danych do magazynu użytkownika;
- śledzenia zmian ilości i decyzji biznesowych;
- obsługi przesunięć i wysyłek między oddziałami;
- zmniejszenia ryzyka błędów wynikających z ręcznych arkuszy lub niespójnych narzędzi.

Warto dodać przykładowy scenariusz:

1. Pracownik sprawdza dostępność produktu.
2. Tworzy prośbę o wysyłkę.
3. System znajduje magazyn źródłowy z wystarczającym zapasem.
4. Kierownik zatwierdza wysyłkę.
5. Towar przechodzi przez statusy `Wysłana`, `W drodze`, `Odebrana`.
6. System aktualizuje stany i zapisuje historię.

## 4. Zakres systemu

Zrealizowane moduły:

- logowanie i profil użytkownika;
- role i uprawnienia;
- zarządzanie użytkownikami;
- produkty, kategorie i jednostki miary;
- magazyny i lokalizacje;
- kontrahenci;
- stany magazynowe;
- ruchy magazynowe;
- operacje PW/RW/MM, korekty i inwentaryzacja;
- wysyłki między magazynami;
- rezerwacje stanów;
- dashboard magazynowy;
- audit log;
- bot symulujący aktywność.

Poza zakresem albo jako przyszły rozwój:

- raporty PDF/CSV;
- integracja z fizycznymi skanerami kodów kreskowych;
- zaawansowane prognozowanie zapasów;
- integracja z ERP/księgowością;
- aplikacja mobilna;
- powiadomienia e-mail/SMS.

## 5. Technologie

Backend:

- .NET 10;
- ASP.NET Core Web API;
- Entity Framework Core;
- SQL Server;
- Mediator/CQRS;
- JWT Bearer Authentication;
- Swagger/OpenAPI;
- xUnit.

Frontend:

- Angular 21;
- Angular Material;
- RxJS;
- NgRx Signals;
- TypeScript;
- Vitest;
- ESLint i Stylelint.

Narzędzia pomocnicze:

- `MagazineWarehouseBot` jako symulator ruchu;
- skrypt seedujący dane demonstracyjne;
- migracje EF Core.

## 6. Architektura systemu

Warto pokazać diagram warstw:

```text
Angular SPA
    |
REST/JSON + JWT
    |
ASP.NET Core Controllers
    |
Application / CQRS / Mediator
    |
Domain
    |
Infrastructure / EF Core
    |
SQL Server
```

Opisz odpowiedzialności:

- kontrolery przyjmują żądania HTTP i sprawdzają uprawnienia;
- handlery komend i zapytań wykonują logikę aplikacyjną;
- domena zawiera encje i stałe biznesowe;
- infrastruktura mapuje domenę na bazę danych;
- SPA jest klientem operacyjnym dla użytkowników;
- bot działa jak zewnętrzny klient API.

## 7. Model danych

W pracy warto umieścić ERD z encjami:

- `Product`, `Category`, `UnitOfMeasure`;
- `Warehouse`, `Location`;
- `Inventory`;
- `Shipment`, `ShipmentItem`;
- `StockReservation`;
- `WarehouseOperation`, `WarehouseOperationItem`;
- `StockMovement`;
- `AuditLog`;
- `User`, `Role`, `Permission`, `RolePermission`;
- `Contractor`;
- `DocumentNumberSequence`.

Najważniejsze reguły do opisania:

- `Inventory.AvailableQuantity = Quantity - ReservedQuantity`;
- `ReservedQuantity <= Quantity`;
- para `ProductId + LocationId` jest unikalna;
- użytkownik niebędący administratorem musi mieć przypisany magazyn;
- wysyłka nie może mieć tego samego magazynu źródłowego i docelowego;
- numery dokumentów są generowane per magazyn, typ i rok;
- `RolePermission` ma klucz złożony.

## 8. Autoryzacja i bezpieczeństwo

Uwzględnij:

- JWT jako mechanizm sesji;
- haszowanie haseł PBKDF2/SHA512;
- role: Administrator, Kierownik, Pracownik;
- uprawnienia typu `products.read`, `inventory.manage`, `shipments.approve`;
- `HasPermission` jako atrybut zabezpieczający endpointy;
- administrator ma pełny dostęp bez wpisów w `RolePermissions`;
- izolacja magazynu przez `IWarehouseContext`;
- nagłówek `X-Warehouse-Id` dla administratora.

W części krytycznej napisz, że w wersji produkcyjnej sekrety muszą być poza repozytorium, a domyślne konto administratora nie powinno być aktywne w konfiguracji produkcyjnej.

## 9. Kluczowe procesy biznesowe

### Stany magazynowe

Opisz:

- dodawanie stanu produktu w lokalizacji;
- edycję ilości całkowitej i zarezerwowanej;
- walidację wartości nieujemnych;
- automatyczny zapis `StockMovement`;
- audit log przy ważnych zmianach.

### Wysyłka między magazynami

Proces:

1. Utworzenie wysyłki albo prośby o wysyłkę.
2. Rezerwacja towaru w magazynie źródłowym.
3. Zatwierdzenie przez osobę z `shipments.approve`.
4. Zużycie rezerwacji i odjęcie towaru ze źródła.
5. Oznaczenie jako `W drodze`.
6. Odbiór z checklistą.
7. Dodanie towaru do lokalizacji przyjęcia w magazynie docelowym.
8. Zapis historii zdarzeń.

### Operacje PW/RW/MM

Opisz wspólny endpoint `POST /api/WarehouseOperations/complete` i typy:

- `InternalReceipt` - przyjęcie wewnętrzne;
- `InternalIssue` - rozchód wewnętrzny;
- `InternalTransfer` - przesunięcie międzymiejscowe;
- `Correction` - korekta;
- `InventoryCount` - inwentaryzacja.

## 10. Interfejs użytkownika

Opisz role ekranów:

- ekran logowania;
- główna nawigacja;
- dashboard;
- katalog produktów;
- stany magazynowe;
- wysyłki;
- operacje magazynowe;
- zarządzanie słownikami;
- użytkownicy, role i uprawnienia;
- profil użytkownika.

Warto dodać zrzuty ekranów z podpisami:

- logowanie;
- dashboard;
- lista produktów;
- formularz produktu;
- stany magazynowe;
- tworzenie wysyłki;
- odbiór wysyłki z checklistą;
- role i uprawnienia;
- audit log;
- panel bota.

## 11. API

W pracy nie trzeba przepisywać wszystkich endpointów bardzo szczegółowo, ale warto pokazać tabelę głównych grup:

- `/api/Auth`;
- `/api/Products`;
- `/api/Inventory`;
- `/api/WarehouseOperations`;
- `/api/Shipments`;
- `/api/StockMovements`;
- `/api/WarehouseDashboard`;
- `/api/Users`;
- `/api/Roles`;
- `/api/Permissions`.

Przykład kontraktu do opisania:

```http
POST /api/Shipments/{id}/receive
Authorization: Bearer <token>
Content-Type: application/json

{
  "checkedItemIds": ["..."],
  "notes": "Brak uszkodzeń"
}
```

## 12. Testowanie i weryfikacja

Uwzględnij wyniki z audytu:

- backend build: OK;
- backend testy: 7/7;
- bot build: OK;
- frontend build: OK;
- frontend lint: OK;
- frontend testy: 24/24.

Opisz typy testów:

- testy jednostkowe handlerów aplikacyjnych;
- testy routingu i guardów Angular;
- testy usług i komponentów;
- testy manualne przez Swagger i SPA;
- testy scenariuszy demonstracyjnych z botem.

Warto uczciwie wskazać braki:

- brak pełnych testów integracyjnych z SQL Server;
- brak testów e2e w przeglądarce;
- za mało testów procesów magazynowych.

## 13. Dane demonstracyjne

Opisz skrypt `MagazineAPI/scripts/seed-prosperous-multi-branch.sql`:

- role i uprawnienia;
- magazyny i lokalizacje;
- produkty;
- stany;
- kontrahenci;
- użytkownicy demonstracyjni.

Wyjaśnij, że dane demo pozwalają szybko pokazać procesy bez ręcznego wprowadzania pełnej bazy.

## 14. Symulator bota

Bot jest ciekawym elementem do pracy, bo pokazuje, że API może być używane nie tylko przez SPA.

Opisz:

- osobną aplikację ASP.NET Core;
- panel do konfiguracji kont;
- logowanie przez `/api/Auth/login`;
- wykonywanie akcji zgodnie z uprawnieniami;
- tworzenie produktów, stanów i wysyłek;
- generowanie aktywności w audit logu i ruchach magazynowych.

W sekcji ograniczeń dopisz, że aktualny bot wymaga dostosowania odbioru wysyłek do checklisty i statusu `W drodze`.

## 15. Wnioski

Wnioski, które warto podkreślić:

- system spełnia podstawowe wymagania aplikacji magazynowej;
- architektura warstwowa ułatwia rozwój i testowanie;
- autoryzacja i izolacja magazynowa są kluczowe dla pracy wielooddziałowej;
- rezerwacje oraz historia ruchów zmieniają projekt z prostego CRUD w system procesowy;
- Angular SPA dobrze oddziela widoki, usługi i guardy;
- projekt można dalej rozwijać w stronę raportowania, integracji i automatyzacji.

## 16. Proponowany spis treści

1. Wstęp
2. Analiza problemu i wymagania
3. Przegląd technologii
4. Projekt architektury systemu
5. Model danych
6. Projekt API i mechanizmów bezpieczeństwa
7. Implementacja backendu
8. Implementacja aplikacji frontendowej
9. Procesy magazynowe i workflow wysyłek
10. Symulator aktywności magazynu
11. Testowanie i wyniki weryfikacji
12. Wdrożenie i konfiguracja
13. Ograniczenia i kierunki dalszego rozwoju
14. Podsumowanie

## 17. Co poprawić przed oddaniem projektu

Najbardziej opłacalne poprawki:

1. Usunąć sekrety z `appsettings.json` i przygotować `appsettings.example.json`.
2. Poprawić odbiór wysyłek w `MagazineWarehouseBot`.
3. Dopisać testy backendu dla wysyłek i operacji magazynowych.
4. Dodać prosty Docker Compose dla SQL Server.
5. Zaktualizować README, żeby nie opisywało zaimplementowanych modułów jako roadmapy.
6. Usunąć nieosiągalny podwójny `return` z handlera operacji magazynowych.

## 18. Materiały do załączników

Do załączników warto dodać:

- diagram architektury;
- diagram ERD;
- tabelę endpointów;
- listę uprawnień;
- przykładowe requesty i response API;
- zrzuty ekranów najważniejszych widoków;
- wyniki komend build/test/lint;
- fragmenty kluczowego kodu: `HasPermission`, `IWarehouseContext`, `ShipmentReservationService`, `CompleteWarehouseOperationCommandHandler`.
