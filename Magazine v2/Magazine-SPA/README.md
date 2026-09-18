# Magazine-SPA

## Gdzie szukać kodu

Kod jest podzielony według funkcji aplikacji: `auth`, `management` i `shipments`. W module zarządzania nazwa obszaru prowadzi przez cały przepływ. Dla użytkowników tabela znajduje się w `src/app/management/pages/users`, formularz i szczegóły w `src/app/management/modals/user`, a wywołania API w `src/app/management/services/user.service.ts`. Produkty, magazyny, lokalizacje i pozostałe obszary mają taki sam układ.

- `pages` — widoki list i jawnie zapisane tabele Angular Material;
- `modals` — osobne dialogi formularzy, szczegółów i potwierdzeń;
- `services` — proste wywołania HTTP, po jednym serwisie na obszar;
- `models` — typy danych zwracanych przez API;
- `management.routes.ts` — adresy ekranów i wymagane uprawnienia;
- `auth` — logowanie, konto, token, interceptor i guardy;
- `shipments` — przyjęcia PZ, wydania WZ i historia.

Pełna checklista i ocena czytelności znajdują się w `MagazineAPI/docs/audyt-projektu.md`.

## Aktualne widoki zarządzania

Pod `/main/management` znajduje się lista ekranów dla produktów, kategorii, jednostek miary, magazynów, lokalizacji, stanów magazynowych, kontrahentów, użytkowników, ról i uprawnień. Przypisania uprawnień są edytowane multiselectem bezpośrednio w formularzu roli. Każdy obszar ma własny komponent strony w `src/app/management/pages`, własną tabelę Angular Material i jawnie zapisane kolumny. Wszystkie kolumny danych można sortować. Produkty są sortowane i stronicowane przez API, a pozostałe listy przez `MatSort` i `MatPaginator` w SPA. Każdy obszar ma także własny modal ze szczegółami, formularzem i potwierdzeniem usunięcia w `src/app/management/modals`. Pola i walidacja formularzy są zapisane bezpośrednio w komponentach poszczególnych encji. Karty i akcje są pokazywane zgodnie z uprawnieniami użytkownika.

Pod `/main/shipments` znajduje się osobny moduł **Wysyłki**. Pracownik może odebrać PZ przez skan GUID albo utworzyć szkic WZ do innego oddziału. Formularz WZ nie pozwala wybrać kontrahenta ani magazynu źródłowego jako docelowego. Pracownik nie ma dostępu do list PZ/WZ ani historii. Kierownik widzi listy i historię oraz zatwierdza dokumenty.

Pod `/main/account` użytkownik widzi swoje dane, rolę, przypisany magazyn i uprawnienia. Może zmienić login, imię, nazwisko, e-mail oraz hasło (po podaniu obecnego hasła). Ustawienia konta i wylogowanie znajdują się w menu użytkownika w prawym górnym rogu. Pracownik oraz kierownik widzą przypisany magazyn. Administrator wybiera aktywny magazyn z listy z wyszukiwarką, a wybór jest zapisywany w przeglądarce i przekazywany do API w nagłówku `X-Warehouse-Id`. Zmiana wyboru odświeża dane wybranej sekcji. Po zmianie danych konta API zwraca nowy token z aktualnymi danymi.

Poniższy tekst jest wcześniejszym briefem projektowym. Opisuje również funkcje docelowe, których API jeszcze nie udostępnia.

> Ten dokument jest gotowym promptem dla projektanta UI/UX lub agenta tworzącego frontend. Źródłem prawdy dla funkcji jest API znajdujące się w sąsiednim projekcie `MagazineAPI`.

## Zadanie

Zaprojektuj spójny, nowoczesny i możliwy do wdrożenia interfejs aplikacji **Magazine** — wewnętrznego systemu do obsługi stanów magazynowych w prosperującej, wielooddziałowej firmie. Aplikacja ma pomagać pracownikom szybko odnajdywać produkty i wykonywać codzienne operacje, a kierownikom i administratorom kontrolować oddziały, dane podstawowe, użytkowników, role oraz uprawnienia.

Przygotuj design całej aplikacji, nie tylko pojedynczego dashboardu. Projekt powinien wyglądać jak dojrzały produkt B2B używany codziennie na komputerach magazynowych i laptopach biurowych. Priorytetami są czytelność, szybkość pracy, dobra hierarchia informacji i niewielka liczba kliknięć.

Jeżeli pracujesz bezpośrednio w repozytorium, zaprojektuj i zaimplementuj UI w istniejącej aplikacji Angular. Nie zmieniaj kontraktów API i nie twórz fikcyjnych wywołań do nieistniejących endpointów.

## Kontekst produktu

- Interfejs i wszystkie komunikaty mają być w języku polskim.
- Waluta: PLN, format liczb i dat zgodny z `pl-PL`.
- Ilości mogą mieć trzy miejsca po przecinku, ponieważ system obsługuje sztuki, opakowania, kilogramy, litry i metry.
- Firma posiada cztery oddziały: Warszawa, Poznań, Kraków i Gdańsk.
- Każdy oddział ma lokalizacje takie jak przyjęcie `REC-01`, sektory składowania `A-01` i `B-01`, kompletacja `PICK-01` oraz zwroty `RET-01`.
- Katalog obejmuje m.in. elektronikę, wyposażenie biura, BHP, narzędzia, opakowania, chemię gospodarczą i artykuły sezonowe.
- System rozróżnia stan całkowity, zarezerwowany oraz dostępny.
- Logo w `public/logo.png` przedstawia trzy paczki połączone strzałkami. Zachowaj jego proporcje i monochromatyczny charakter.

## Użytkownicy i dostęp

Interfejs musi reagować na rolę oraz kody uprawnień zwracane przez API. Ukrywaj niedostępne moduły i akcje, ale nie projektuj uprawnień jako jedynego zabezpieczenia — API pozostaje źródłem autoryzacji.

### Administrator

- Ma pełny dostęp do wszystkich funkcji.
- Zarządza użytkownikami, rolami, uprawnieniami, słownikami, magazynami i danymi biznesowymi.
- Rola o ID `10000000-0000-0000-0000-000000000001` posiada wszystkie uprawnienia, również bez osobnych przypisań.

### Kierownik

- Przegląda i zarządza produktami oraz stanami.
- Przegląda magazyny i kontrahentów.
- Zarządza szkicami PZ i WZ oraz zatwierdza dokumenty przygotowane przez pracowników.
- Nie zarządza użytkownikami, rolami ani słownikami, jeśli nie otrzyma odpowiedniego uprawnienia.

### Pracownik

- Przegląda produkty i stany.
- Skanuje GUID przesyłki PZ, przegląda zawartość paczki i potwierdza odbiór.
- Tworzy szkic WZ, wybierając inny oddział jako magazyn docelowy.
- Nie ma dostępu do list PZ/WZ, historii, zatwierdzania ani edycji stanów.
- Nie widzi funkcji administracyjnych.

Obsługiwane kody uprawnień:

| Kod                       | Znaczenie w interfejsie                                   |
| ------------------------- | --------------------------------------------------------- |
| `products.read`           | Lista i szczegóły produktów                               |
| `products.manage`         | Dodawanie, edycja i wycofywanie produktów                 |
| `inventory.read`          | Stany całkowite, zarezerwowane i dostępne                 |
| `inventory.manage`        | Bezpośrednia edycja stanów magazynowych                   |
| `stock-documents.manage`  | Tworzenie, edycja i usuwanie szkiców PZ oraz WZ           |
| `stock-documents.approve` | Zatwierdzanie PZ i WZ oraz aktualizacja stanu             |
| `stock-documents.read`    | Listy PZ/WZ, szczegóły i historia                         |
| `stock-documents.receive` | Skanowanie, podgląd paczki i potwierdzenie odbioru PZ     |
| `stock-shipments.create`  | Tworzenie szkiców WZ między oddziałami                    |
| `warehouses.read`         | Magazyny oraz ich lokalizacje                             |
| `warehouses.manage`       | Edycja magazynów i lokalizacji                            |
| `contractors.read`        | Dostawcy i odbiorcy                                       |
| `contractors.manage`      | Edycja kontrahentów                                       |
| `users.read`              | Lista i szczegóły użytkowników                            |
| `users.manage`            | Edycja i usuwanie użytkowników                            |
| `dictionaries.manage`     | Kategorie i jednostki miary                               |
| `roles.manage`            | Role i macierz uprawnień; dostęp systemowy administratora |

## Kierunek wizualny

Stwórz jasny, spokojny i profesjonalny interfejs klasy enterprise. Ma kojarzyć się z porządkiem, logistyką i kontrolą, ale nie może wyglądać jak surowy panel administracyjny ani gotowy, niezmieniony template Material Design.

- Bazuj na ciemnym graficie logo, bieli i chłodnych szarościach.
- Kolor główny: nasycony niebieski, używany oszczędnie do aktywnych elementów i głównych akcji.
- Status poprawny/dostępny: zielony; ostrzeżenie/niski stan: bursztynowy; błąd/brak stanu: czerwony; rezerwacja: fioletowy lub niebieskoszary.
- Przykładowe tokeny: `#1F2937` tekst i logo, `#2563EB` primary, `#F5F7FA` tło, `#FFFFFF` powierzchnie, `#D9E1EA` obramowania, `#15803D` sukces, `#B45309` ostrzeżenie, `#B42318` błąd.
- Typografia: Roboto, czytelna skala i wyraźne liczby tabelaryczne. Tekst bazowy nie mniejszy niż 14 px.
- Promień narożników 8–12 px. Cienie bardzo delikatne; separację buduj głównie odstępem, kolorem powierzchni i obramowaniem.
- Stosuj gęstość odpowiednią dla aplikacji operacyjnej: kompaktowe tabele i formularze, ale pola oraz przyciski muszą pozostać wygodne.
- Ikony mają pochodzić z jednego zestawu, najlepiej Material Symbols. Nie używaj emoji jako ikon.
- Unikaj gradientów, szkła, wielkich pustych kart, ozdobnych wykresów bez znaczenia oraz nadmiernej liczby kolorów.

## Układ aplikacji

Zastosuj skalowalny układ zamiast długiego poziomego menu:

- po lewej zwijany panel nawigacji z logo, nazwą „Magazine” i modułami;
- u góry pasek kontekstowy z tytułem strony, breadcrumbs, wyborem oddziału, powiadomieniami i menu użytkownika;
- główna treść na jasnoszarym tle, ograniczona czytelną maksymalną szerokością na dużych ekranach;
- stała i przewidywalna pozycja głównej akcji strony;
- nawigacja mobilna jako wysuwany panel, bez ściskania pełnej tabeli do szerokości telefonu.

Proponowana architektura informacji:

1. Pulpit
2. Produkty
3. Stany magazynowe
4. Operacje magazynowe
5. Magazyny i lokalizacje
6. Kontrahenci
7. Administracja
   - Użytkownicy
   - Role i uprawnienia
   - Kategorie
   - Jednostki miary

Każdy element menu pokazuj tylko wtedy, gdy użytkownik ma odpowiadające mu uprawnienie. Nie dodawaj modułów zamówień, sprzedaży ani dokumentów handlowych — obecny model API i bazy ich nie obsługuje.

## Ekrany do zaprojektowania

### 1. Logowanie

- Spokojny ekran z wyraźnym logo, nazwą produktu i krótkim hasłem opisującym system.
- Formularz zawiera login lub e-mail, hasło, przełącznik widoczności hasła i przycisk „Zaloguj się”.
- Walidacja pól pojawia się przy polach, a błąd autoryzacji w czytelnym komunikacie formularza.
- Przycisk pokazuje stan ładowania i nie pozwala na wielokrotne wysłanie.
- Nie eksponuj publicznej rejestracji na głównym ekranie — to wewnętrzny system firmowy.
- Nie używaj generycznego zdjęcia hali jako dominującego tła. Branding może korzystać z subtelnego motywu paczek, siatki lokalizacji lub przepływu towaru.

### 2. Pulpit

Pokaż najważniejszy obraz sytuacji bez przeładowania informacjami:

- kafle KPI: liczba aktywnych produktów, dostępny stan, ilość zarezerwowana, wartość dostępnego towaru;
- przełącznik zakresu „Wszystkie oddziały” lub konkretny magazyn;
- rozkład stanu między Warszawę, Poznań, Kraków i Gdańsk;
- lista alertów: niski stan, brak towaru, duży udział rezerwacji;
- ostatnie operacje magazynowe;
- szybkie akcje dostępne zgodnie z uprawnieniami: przyjęcie, wydanie, przesunięcie, korekta;
- sekcja „Wymaga uwagi” ważniejsza wizualnie niż dekoracyjne wykresy.

API nie udostępnia jeszcze endpointu dashboardu. Traktuj ten ekran jako docelową koncepcję i użyj danych demonstracyjnych tylko do prezentacji designu. Nie dodawaj fikcyjnego wywołania HTTP.

### 3. Produkty

To główny ekran połączony z `GET /api/Products`.

- Nagłówek „Produkty”, krótki opis i akcja „Dodaj produkt” widoczna przy `products.manage`.
- Wyszukiwanie po nazwie, SKU i kodzie kreskowym.
- Filtry: kategoria, jednostka miary i status aktywności. Jeżeli API nie obsługuje danego filtra, filtruj lokalnie tylko wtedy, gdy dostępny jest pełny zbiór; w innym przypadku pokaż go jako projekt przyszłej funkcji.
- Sortowanie rosnąco/malejąco po nazwie oraz paginacja 10, 25 lub 50 rekordów.
- Kolumny: nazwa z krótkim opisem, SKU, kod kreskowy, jednostka, kategoria, cena zakupu, cena sprzedaży, marża, status i akcje.
- SKU oraz kod kreskowy powinny być łatwe do skopiowania.
- Akcja szczegółów otwiera dialog z danymi produktu.
- Dodawanie i edycja działają w dialogach Angular Material.
- Status „Aktywny” lub „Nieaktywny” prezentuj tekstem i kolorem, nie samym kolorem.
- Pokaż komplet stanów: ładowanie, brak produktów, brak wyników wyszukiwania, błąd i ponowienie.

Dane zwracane dla produktu: `productId`, `name`, `sku`, `barcode`, `description`, `unitOfMeasure`, `category`, `purchasePrice`, `salePrice`, `isActive`.

### 4. Stany magazynowe

- Globalna wyszukiwarka produktu oraz filtry magazynu, lokalizacji, kategorii i statusu zapasu.
- Najważniejsze kolumny: produkt, SKU, magazyn, lokalizacja, stan całkowity, rezerwacja i stan dostępny.
- Ilość dostępna ma najwyższy priorytet wizualny.
- Dodaj czytelne oznaczenia „Dostępny”, „Niski stan” i „Brak”, bez polegania wyłącznie na kolorze.
- Szczegóły produktu pokazują rozkład ilości między oddziały i lokalizacje.
- Na telefonie zamiast szerokiej tabeli użyj kart z najważniejszymi wartościami i rozwijanymi szczegółami.

### 5. Operacje magazynowe

Zaprojektuj wspólny, prosty przepływ dla przyjęcia, wydania, przesunięcia i korekty:

1. wybór typu operacji;
2. zeskanowanie lub wyszukanie produktu;
3. wybór magazynu i lokalizacji źródłowej/docelowej;
4. podanie ilości wraz z jednostką;
5. podsumowanie i potwierdzenie.

Podstawowy przepływ PZ i WZ jest wdrożony. Pracownik odbiera PZ przez skan GUID albo tworzy międzyoddziałowy szkic WZ. Kierownik zatwierdza dokument, a dopiero zatwierdzenie aktualizuje stan. Przesunięcia i korekty pozostają dalszym etapem.

### 6. Magazyny i lokalizacje

- Lista czterech oddziałów z adresem, opisem, liczbą lokalizacji i podsumowaniem stanów.
- Szczegóły oddziału zawierają lokalizacje `REC-01`, `A-01`, `B-01`, `PICK-01` i `RET-01`.
- Dla lokalizacji pokaż opis, liczbę SKU, stan całkowity i wykorzystanie.
- Użytkownik z `warehouses.manage` otrzymuje akcje dodawania i edycji magazynu lub lokalizacji.
- Nie projektuj mapy geograficznej jako głównego widoku — ważniejsza jest struktura oddział → lokalizacje → zapas.

### 7. Kontrahenci

- Lista i szczegóły kontrahentów z typem: dostawca, odbiorca lub oba.
- Pola: nazwa, NIP, typ, e-mail, telefon i adres.
- Zapewnij wyszukiwanie, filtr typu, kopiowanie danych kontaktowych oraz czytelne akcje edycji.
- Formularz powinien obsługiwać walidację NIP, e-maila i wymaganych pól.

### 8. Użytkownicy

- Lista kont: imię i nazwisko, login, e-mail, rola i akcje.
- Formularz edycji: login, imię, nazwisko, e-mail, opcjonalna zmiana hasła i rola.
- Usuwanie wymaga dialogu potwierdzenia z nazwą użytkownika i opisem konsekwencji.
- Nie pozwalaj wizualnie na usunięcie własnego konta ani administratora, jeśli operacja jest zabroniona przez API.
- Rozróżnij komunikaty `400`, `403`, `404` i `409`; konflikt loginu lub e-maila pokaż przy odpowiednim polu.

### 9. Role i uprawnienia

- Lista ról z nazwą, liczbą uprawnień i informacją o pełnym dostępie.
- Szczegóły jako czytelna macierz lub pogrupowana lista przełączników: Produkty, Stany, Magazyny, Kontrahenci, Użytkownicy i Słowniki.
- Każde uprawnienie ma nazwę i opis, a nie tylko kod techniczny.
- Dla administratora pokaż „Pełny dostęp” i zablokowane kontrolki z wyjaśnieniem systemowego charakteru roli.
- Uprawnienia roli są wybierane multiselectem w formularzu dodawania i edycji roli.
- Zmiany zapisuj jedną, wyraźną akcją; pokaż ostrzeżenie o niezapisanych zmianach.

### 10. Kategorie i jednostki miary

- Proste widoki słownikowe dostępne przy `dictionaries.manage`.
- Kategorie: nazwa i opis.
- Jednostki: nazwa i symbol, np. sztuka / `szt.`, kilogram / `kg`, litr / `l`.
- Edycję można wykonać w małym panelu bocznym, ponieważ formularze są krótkie.

### 11. Profil użytkownika

- Dane z `GET /api/Auth/me`: login, imię, nazwisko, e-mail i rola.
- Lista aktywnych uprawnień z `GET /api/Auth/me/permissions`.
- Menu użytkownika zawiera profil i wylogowanie.

## Kontrakty istniejącego API

Nie zmieniaj nazw pól ani tras podczas tworzenia warstwy UI.

| Metoda i trasa                               | Zastosowanie                                               |
| -------------------------------------------- | ---------------------------------------------------------- |
| `POST /api/Auth/login`                       | Logowanie; body: `login`, `password`; odpowiedź: `token`   |
| `GET /api/Auth/IsAuthenticated`              | Sprawdzenie sesji                                          |
| `GET /api/Auth/me`                           | Dane zalogowanego użytkownika                              |
| `GET /api/Auth/me/permissions`               | Lista kodów uprawnień                                      |
| `GET /api/Products`                          | Produkty; parametry: `pageSize`, `page`, `search`, `order` |
| `GET /api/StockDocuments`                    | Lista PZ albo WZ; `type`, `page`, `pageSize`, `search`     |
| `GET /api/StockDocuments/history`            | Historia zatwierdzonych PZ i WZ z paginacją                |
| `GET /api/StockDocuments/scan/{id}`          | Zawartość jednej zeskanowanej przesyłki PZ                 |
| `POST /api/StockDocuments/{id}/receive`      | Potwierdzenie odbioru PZ bez zmiany stanu                  |
| `GET /api/StockDocuments/shipment-page-data` | Dane formularza WZ między oddziałami                       |
| `POST /api/StockDocuments/shipments`         | Utworzenie szkicu WZ przez pracownika                      |
| `POST /api/StockDocuments`                   | Utworzenie szkicu dokumentu z pozycjami                    |
| `PUT /api/StockDocuments/{id}`               | Edycja szkicu                                              |
| `POST /api/StockDocuments/{id}/complete`     | Zatwierdzenie dokumentu i aktualizacja stanu               |
| `GET /api/Roles`                             | Role wraz z kodami uprawnień                               |
| `GET /api/Roles/permissions`                 | Katalog uprawnień z nazwami i opisami                      |
| `PUT /api/Roles/{roleId}/permissions`        | Zapis kodów uprawnień roli                                 |
| `PUT /api/Users/{id}`                        | Edycja konta i roli użytkownika                            |
| `DELETE /api/Users/{id}`                     | Usunięcie użytkownika                                      |

Odpowiedź listy produktów ma format:

```json
{
  "total": 18,
  "list": [],
  "page": 1,
  "pageSize": 10
}
```

Autoryzowane żądania wysyłają token jako `Authorization: Bearer <token>`. Przy wyborze zakresu administratora interceptor dodaje także `X-Warehouse-Id`.

## Stany i zachowanie interfejsu

Każdy widok pobierający dane powinien mieć zaprojektowane:

- skeleton lub subtelny stan ładowania bez skakania layoutu;
- pusty stan z konkretną podpowiedzią i akcją, jeśli użytkownik może coś dodać;
- osobny stan braku wyników po filtracji;
- błąd z komunikatem i akcją „Spróbuj ponownie”;
- `401`: wyczyszczenie sesji i przekierowanie do logowania z możliwością powrotu;
- `403`: strona „Brak dostępu” z bezpiecznym powrotem, nie pusty ekran;
- potwierdzenie sukcesu jako krótki snackbar, bez zasłaniania ważnych kontrolek;
- potwierdzenie każdej nieodwracalnej lub ryzykownej akcji.

Formularze waliduj na bieżąco po interakcji z polem. Zachowuj wpisane dane po błędzie serwera. Przyciski podczas zapisu mają być zablokowane i pokazywać postęp.

## Responsywność i dostępność

- Projektuj przede wszystkim dla szerokości 1440 px, sprawdź również 1024 px i 390 px.
- Na mniejszych ekranach zachowaj dostęp do najważniejszych akcji i filtrów.
- Tabele mogą przejść w karty lub priorytetyzowane kolumny; nie stosuj poziomego przewijania jako jedynego rozwiązania.
- Wszystkie elementy interaktywne muszą być dostępne klawiaturą i mieć widoczny focus.
- Minimalny cel dotykowy: 44 × 44 px.
- Zapewnij kontrast zgodny z WCAG AA.
- Ikona, kolor i tekst powinny wspólnie przekazywać status.
- Dialogi i panele muszą mieć logiczną kolejność fokusu, etykiety oraz możliwość zamknięcia klawiaturą.

## Ograniczenia techniczne

Aktualny frontend wykorzystuje:

- Angular 21 ze standalone components;
- Angular Material 3 i Angular CDK;
- NgRx Signals;
- SCSS;
- Material Symbols;
- leniwe ładowanie tras.

Wykorzystaj istniejący stack i komponenty Material jako dostępne prymitywy, ale nadaj im spójny system wizualny przez tokeny motywu i własne style. Nie dodawaj drugiego frameworka komponentów. Zachowaj obsługę JWT, guardy, interceptor, reaktywne formularze oraz istniejące integracje z API.

Komponenty powtarzalne — nagłówki stron, filtry, statusy, potwierdzenia, puste stany i tabele — powinny korzystać ze wspólnego API i tych samych tokenów. Nie duplikuj osobnych wariantów stylistycznych w każdym module.

## Oczekiwany rezultat

Przygotuj kompletny, spójny design high-fidelity obejmujący:

1. fundamenty systemu wizualnego: kolory, typografię, odstępy, promienie, stany i ikony;
2. główny layout i wariant zwiniętej nawigacji;
3. wszystkie opisane ekrany w wersji desktopowej;
4. kluczowe widoki mobilne: logowanie, pulpit, produkty, stany i operacja magazynowa;
5. warianty loading, empty, no-results, error, 403 oraz confirm;
6. zachowanie komponentów i przepływy między ekranami;
7. realistyczne polskie treści oparte na danych domenowych, bez lorem ipsum;
8. implementację responsywną i dostępną, jeśli zadanie jest wykonywane w kodzie.

Design powinien sprawiać wrażenie jednego produktu, a nie zbioru niezależnych ekranów. Użytkownik ma od razu rozumieć, gdzie się znajduje, jakiego oddziału dotyczą dane, co wymaga uwagi i jaka jest najważniejsza akcja na danej stronie.
