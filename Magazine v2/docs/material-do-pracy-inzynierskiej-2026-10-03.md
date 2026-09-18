# Co uwzględnić w pracy inżynierskiej - Magazine v2

## Proponowany tytuł

Projekt i implementacja systemu magazynowego z obsługą wielu oddziałów, dokumentów magazynowych, wysyłek między magazynami oraz symulatorem pracy użytkowników.

## Cel pracy

Celem pracy jest zaprojektowanie i wykonanie aplikacji wspierającej obsługę magazynu w strukturze wielooddziałowej. System umożliwia zarządzanie produktami, stanami magazynowymi, lokalizacjami, dokumentami PW/RW/MM, przesunięciami, wysyłkami między magazynami, użytkownikami, rolami oraz uprawnieniami. Dodatkowo projekt zawiera bota symulującego codzienną pracę administratora, kierownika i pracowników.

## Problem badawczo-inżynierski

W pracy warto pokazać, że problemem nie jest tylko prosta ewidencja produktów. System musi rozwiązać kilka trudniejszych zagadnień:

- wiele magazynów i oddziałów,
- ograniczenie widoczności danych do magazynu pracownika,
- centralny administrator z dostępem do wszystkich oddziałów,
- rezerwacje stanów pod wysyłki,
- historia ruchów magazynowych,
- dokumenty magazynowe wpływające na stan,
- role i uprawnienia,
- spójność danych przy operacjach biznesowych,
- wydajność przy tabelach i raportach,
- automatyczna symulacja pracy systemu przez bota.

## Architektura systemu

Opisz podział na trzy główne części:

- `MagazineAPI` - backend REST API w ASP.NET Core.
- `Magazine-SPA` - frontend Angular.
- `MagazineWarehouseBot` - aplikacja symulująca działania użytkowników.

W backendzie warto omówić warstwy:

- Domain - encje, enumy, reguły domenowe.
- Application - komendy, zapytania, DTO, logika przypadków użycia.
- Infrastructure - EF Core, konfiguracje encji, repozytoria, migracje.
- API - kontrolery, autoryzacja, JWT, obsługa błędów.
- Event - obsługa zdarzeń, historia wysyłek i zmian.

## Model domenowy

Najważniejsze encje do opisania:

- `Product` - produkt, SKU, kod kreskowy, kategoria, jednostka.
- `Warehouse` - magazyn lub oddział.
- `Location` - lokalizacja w magazynie.
- `Inventory` - stan produktu w lokalizacji.
- `StockReservation` - rezerwacja stanu.
- `StockMovement` - ruch magazynowy.
- `WarehouseOperation` - dokument PW/RW/MM/korekta/inwentaryzacja.
- `Shipment` - wysyłka między magazynami.
- `ShipmentItem` - pozycja wysyłki.
- `User`, `Role`, `Permission` - bezpieczeństwo i kontrola dostępu.
- `AuditLog` - historia operacji.
- `DocumentNumberSequence` - numeracja dokumentów.

## Role użytkowników

W pracy opisz trzy główne poziomy:

- Administrator - dostęp globalny, konfiguracja systemu, użytkownicy, role, słowniki.
- Kierownik - zarządzanie operacjami magazynowymi i akceptacja wysyłek.
- Pracownik - praca operacyjna w przypisanym magazynie.

Warto zaznaczyć, że administrator może przełączać aktywny magazyn, a pracownik jest przypisany do konkretnego oddziału.

## Główne moduły funkcjonalne

### Dashboard

Opis:

- liczba produktów,
- suma stanów,
- rezerwacje,
- dostępna ilość,
- wysyłki według statusów,
- alerty niskiego stanu.

Warto pokazać, że dashboard korzysta z kontekstu aktywnego magazynu.

### Produkty i słowniki

Opis:

- kartoteka produktów,
- SKU i kody kreskowe,
- kategorie,
- jednostki miary,
- aktywność produktów.

### Magazyny i lokalizacje

Opis:

- obsługa wielu oddziałów,
- lokalizacje składowania,
- unikalność kodu lokalizacji w magazynie,
- wybór aktywnego magazynu w interfejsie.

### Stany magazynowe

Opis:

- ilość całkowita,
- ilość zarezerwowana,
- ilość dostępna,
- powiązanie produktu z lokalizacją.

### Dokumenty PW/RW/MM

Opis:

- PW - przyjęcie wewnętrzne,
- RW - rozchód wewnętrzny,
- MM - przesunięcie międzymagazynowe lub wewnętrzne,
- korekta,
- inwentaryzacja,
- generowanie numeru dokumentu,
- księgowanie zmian na stanie,
- zapis ruchów magazynowych.

To jest bardzo dobry rozdział praktyczny, bo pokazuje logikę biznesową systemu.

### Wysyłki między magazynami

Opis:

- utworzenie wysyłki,
- prośba o wysyłkę,
- akceptacja,
- status `Wysłana`,
- status `W drodze`,
- odbiór,
- przyjęcie na stan,
- historia,
- WZ i etykieta,
- QR/kod kreskowy.

Warto pokazać diagram stanów wysyłki.

### Historia i audyt

Opis:

- ruchy magazynowe,
- audit log,
- historia wysyłek,
- kto i kiedy wykonał operację.

### WarehouseBot

Opis:

- symulacja użytkowników,
- automatyczne działania administratora, kierownika i pracowników,
- kilku pracowników na oddział,
- obsługa API zamiast bezpośredniej manipulacji bazą,
- generowanie danych i ruchu w systemie.

To można przedstawić jako narzędzie testowo-demonstracyjne.

## Bezpieczeństwo

Uwzględnij:

- JWT,
- haszowanie haseł,
- role i uprawnienia,
- permission guard w Angularze,
- atrybuty `HasPermission` w API,
- administrator jako rola z globalnym dostępem,
- ograniczenie danych po aktywnym magazynie.

## Baza danych

Opisz:

- SQL Server,
- EF Core,
- migracje,
- relacje między encjami,
- indeksy,
- ograniczenia unikalności,
- check constraints,
- automatyczne migracje przy starcie API.

Warto dodać diagram ERD z głównymi tabelami.

## Wydajność

Uwzględnij realne problemy i poprawki:

- nie pobierać całych tabel do pamięci,
- używać `IQueryable` i filtrowania po stronie SQL,
- dodawać limity i paginację,
- nie ładować `page-data`, jeżeli formularz nie został otwarty,
- stosować indeksy dla dużych tabel,
- pilnować renderowania Angulara po odpowiedziach HTTP.

Jako przykład opisz poprawkę wysyłek:

- osobny endpoint `pending`,
- ograniczenie liczby requestów na starcie,
- indeksy po statusie, magazynie i dacie,
- leniwe ładowanie danych formularza.

## Testowanie

Opisz aktualny stan:

- testy jednostkowe backendu,
- testy Angulara,
- build frontendu,
- build backendu,
- build WarehouseBota.

Opisz też, co należałoby dodać:

- testy E2E,
- testy integracyjne API + baza,
- testy scenariuszy uprawnień,
- testy procesu wysyłki od utworzenia do odbioru,
- testy WarehouseBota na bazie testowej.

## Proponowana struktura pracy

1. Wstęp
2. Cel i zakres pracy
3. Analiza wymagań systemu magazynowego
4. Projekt architektury systemu
5. Model danych i baza danych
6. Projekt mechanizmu ról i uprawnień
7. Implementacja backendu REST API
8. Implementacja aplikacji Angular
9. Obsługa dokumentów magazynowych i wysyłek
10. WarehouseBot jako symulator działania systemu
11. Testowanie i walidacja
12. Problemy napotkane podczas implementacji
13. Możliwości dalszego rozwoju
14. Podsumowanie

## Co warto pokazać na obronie

- logowanie,
- wybór magazynu,
- dashboard,
- dodanie produktu,
- utworzenie dokumentu PW,
- sprawdzenie stanu,
- utworzenie wysyłki,
- akceptacja wysyłki,
- oznaczenie jako w drodze,
- odbiór wysyłki,
- ruchy magazynowe,
- audit log,
- działanie WarehouseBota.

## Uczciwe ograniczenia projektu

W pracy warto napisać je świadomie:

- nie wszystkie listy mają pełną paginację serwerową,
- testy E2E nie są jeszcze kompletne,
- `npm audit` wskazuje zależności wymagające przeglądu,
- część prostych słowników zakłada małą liczbę rekordów,
- bot wymaga osobnego scenariusza testu integracyjnego.

## Najmocniejsze strony projektu

- szeroki zakres funkcjonalny,
- rzeczywista logika magazynowa,
- wiele oddziałów,
- role i uprawnienia,
- dokumenty magazynowe,
- wysyłki i rezerwacje,
- historia ruchów,
- bot symulujący realnych użytkowników,
- architektura warstwowa,
- migracje i constraints w bazie.

