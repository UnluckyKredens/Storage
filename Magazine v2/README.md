# Magazine v2

System magazynowy dla sieci magazynów: katalog przedmiotów, stany i rezerwacje, dokumenty PW/RW/MM, wysyłki między magazynami,
zamówienia zewnętrzne od dostawców, sklep internetowy z pakowaniem zamówień, role i uprawnienia, audyt oraz dashboard dla kierownika.

```
 Sklep (StorefrontDemo, sw.js) ──┐
                                 ├──►  API  ASP.NET Core (.NET 10)  ──►  SQL Server: baza główna + baza historii
 Panel pracowników (Angular 21) ─┘     http://localhost:5392              (dwie bazy, przepływ zdarzeń przez outbox)
 http://localhost:4200
```

| Część | Katalog | Technologia |
|---|---|---|
| API | `MagazineAPI/` | ASP.NET Core, EF Core, Mediator (CQRS), JWT, SQL Server |
| Panel | `Magazine-SPA/` | Angular 21, Angular Material, ApexCharts |
| Sklep demo | `StorefrontDemo/` | czysty HTML/JS na bibliotece `sw.js` serwowanej przez API |
| Dane testowe | `seeds/` | katalog przedmiotów i scenariusz do przejścia ręcznie |

---

## 1. Wymagania

| Narzędzie | Wersja | Po co |
|---|---|---|
| .NET SDK | 10 | uruchomienie API i migracji |
| Node.js + npm | 20+ (testowane na 24) | panel Angular |
| SQL Server | 2019+ (Express, Developer lub Standard) | baza |
| `sqlcmd` | dowolny (instaluje się z SQL Server) | ładowanie danych testowych (opcjonalnie) |
| Serwer plików statycznych | np. IIS lub `py -m http.server` | sklep demo (opcjonalnie) |

Polecenia poniżej są zapisane dla **PowerShella** na Windows Server.

## 2. Szybki start

### Krok 1. Baza SQL Server

Zainstaluj SQL Server (instancja domyślna `localhost` albo nazwana, np. `.\SQLEXPRESS`) i upewnij się, że:

- włączony jest protokół **TCP/IP** (SQL Server Configuration Manager) i usługa została uruchomiona ponownie,
- konto, którego użyje API, może **tworzyć bazy** (np. `sysadmin` lub `dbcreator`), bo migracje tworzą bazy same.
  Dla logowania SQL (`sa`) włącz też tryb uwierzytelniania mieszanego.

Bazy (`Magazine` i `MagazineHistory`) tworzą się same przy pierwszym starcie API, bo uruchamia on migracje.

### Krok 2. Konfiguracja API

Plik `appsettings.json` celowo nie zawiera sekretów. Utwórz obok niego plik **`MagazineAPI/MagazineAPI/appsettings.Local.json`**
(jest w `.gitignore` i ładuje się tylko w środowisku `Development`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Magazine;User Id=sa;Password=<mocne-haslo-sa>;Encrypt=True;TrustServerCertificate=True",
    "HistoryConnection": "Server=localhost;Database=MagazineHistory;User Id=sa;Password=<mocne-haslo-sa>;Encrypt=True;TrustServerCertificate=True"
  },
  "Jwt": { "SecretKey": "<minimum-32-bajty>" },
  "InitialAdmin": { "Enabled": true, "Login": "admin", "Password": "<haslo-admina-min-8-znakow>" }
}
```

Zamiast loginu i hasła SQL możesz użyć logowania Windows: `Server=localhost;Database=Magazine;Integrated Security=True;Encrypt=True;TrustServerCertificate=True`.
Wtedy konto, na którym działa API (przy IIS: tożsamość puli aplikacji), musi mieć dostęp do SQL Servera.

Sekret JWT (minimum 32 bajty) wygenerujesz w PowerShellu:

```powershell
$b = New-Object byte[] 48; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
```

Zamiast pliku można użyć zmiennych środowiskowych (zapis z podwójnym podkreśleniem), np. w PowerShellu:
`$env:ConnectionStrings__DefaultConnection = "..."`, `$env:Jwt__SecretKey = "..."`, `$env:InitialAdmin__Password = "..."`. Na stałe, dla usługi lub IIS,
ustaw je jako zmienne systemowe albo w konfiguracji puli aplikacji.

Bez sekretu JWT, obu connection stringów i (przy `InitialAdmin.Enabled`) hasła administratora API **odmówi startu** z czytelnym komunikatem.

### Krok 3. Uruchom API

```powershell
cd MagazineAPI\MagazineAPI
dotnet run --launch-profile http
```

Przy starcie API wykonuje migracje obu baz i zakłada konto administratora (tylko gdy nie istnieje). **Nic nie kasuje.**
Sprawdź: `http://localhost:5392/swagger` (dokumentacja endpointów, tylko w `Development`).

### Krok 4. Uruchom panel

```powershell
cd Magazine-SPA
npm install
npm start
```

Panel: **http://localhost:4200**. Zaloguj się loginem `admin` i hasłem z kroku 2. Adres API panelu to `http://localhost:5392/api`
(plik `src/environments/environment.development.ts`).

### Krok 5. (opcjonalnie) Sklep demo

```powershell
cd StorefrontDemo
py -m http.server 8081
```

Zamiast Pythona możesz wystawić katalog `StorefrontDemo` jako zwykłą stronę statyczną w IIS.

Sklep: **http://localhost:8081**. Pole „API" na stronie domyślnie wskazuje `http://localhost:5392/api/Storefront`.

### Krok 6. (opcjonalnie) Dane testowe

```powershell
sqlcmd -C -I -f 65001 -S localhost -d Magazine -U sa -P "<mocne-haslo-sa>" -i seeds\seed-computer-parts-items.sql
```

Doda **87 przedmiotów w 13 kategoriach** (części komputerowe) i nic więcej: żadnych magazynów, lokalizacji ani stanów. Dalej idź według
[scenariusza](seeds/scenariusz-czesci-komputerowe.md), który zakłada czteromagazynową sieć (Gdańsk, Kraków, Warszawa, Poznań).
Flaga `-I` jest wymagana, a jeśli Twoja wersja `sqlcmd` nie zna flagi `-C`, pomiń ją. Skrypt można uruchamiać wielokrotnie.

---

## 3. Role i uprawnienia

Są trzy role wbudowane. Uprawnienia modyfikuje administrator na stronie *Zarządzanie → Role*.

| Uprawnienie | Administrator | Kierownik | Pracownik |
|---|:-:|:-:|:-:|
| Podgląd produktów (`products.read`) | ✔ | ✔ | ✔ |
| Zarządzanie produktami (`products.manage`) | ✔ | ✔ | |
| Podgląd stanów (`inventory.read`) | ✔ | ✔ | ✔ |
| Księgowanie dokumentów PW/RW/MM (`inventory.manage`) | ✔ | ✔ | |
| Podgląd magazynów i lokalizacji (`warehouses.read`) | ✔ | ✔ | |
| Zarządzanie magazynami (`warehouses.manage`) | ✔ | | |
| Podgląd kontrahentów (`contractors.read`) | ✔ | ✔ | |
| Podgląd wysyłek (`shipments.read`) | ✔ | ✔ | ✔ |
| Tworzenie wysyłek (`shipments.create`) | ✔ | ✔ | ✔ |
| Akceptacja wysyłek i statystyki przepływu (`shipments.approve`) | ✔ | ✔ | |
| Podgląd zamówień zewnętrznych (`purchase-orders.read`) | ✔ | ✔ | ✔ |
| Tworzenie zamówień zewnętrznych (`purchase-orders.create`) | ✔ | ✔ | ✔ |
| Akceptacja zamówień zewnętrznych (`purchase-orders.approve`) | ✔ | ✔ | |
| Użytkownicy, role, słowniki, kontrahenci (zarządzanie), audyt | ✔ | | |

- **Administrator** nie jest przypisany do magazynu i widzi wszystkie. Aktywny magazyn wybiera selektorem w lewym dolnym rogu panelu.
- **Kierownik i Pracownik** są przypisani do jednego magazynu i widzą tylko jego dane. Selektora nie mają.
- Zalogować się można **loginem lub adresem e-mail**.

## 4. Mapa panelu

Menu boczne: **Dashboard**, **Magazyn** (stany), **Wysyłki**, **Zamówienia** (zewnętrzne), **Pakowanie** (zamówienia ze sklepu), **Zarządzanie**.
Strona *Zarządzanie* zbiera pozostałe moduły, widoczne według uprawnień:

| Moduł | Do czego służy |
|---|---|
| Produkty, Kategorie, Jednostki miary | katalog przedmiotów (SKU, kod EAN, ceny, progi minimum i optimum) |
| Magazyny, Lokalizacje | struktura magazynów (np. `REC-01`, `A-01`, `PICK-01`) |
| Stany magazynowe | ilości, rezerwacje i ilość dostępna per lokalizacja |
| Dokumenty PW/RW/MM | przyjęcia, rozchody, przesunięcia, korekty i inwentaryzacja |
| Ruchy magazynowe | pełna księga zmian stanu z dokumentem źródłowym |
| Wysyłki | wysyłki między magazynami: utworzenie, akceptacja, w drodze, odbiór, anulowanie, prośby |
| Zamówienia zewnętrzne | zamówienia do dostawców, faktura, przyjęcie na stan |
| Zakupy do spakowania | zamówienia ze sklepu: przyjęcie do realizacji i pakowanie |
| Kontrahenci | dostawcy i odbiorcy |
| Użytkownicy, Role, Uprawnienia | konta i dostęp |
| Audit log | historia kluczowych zmian w systemie |

---

## 5. Lista kontrolna do ręcznego sprawdzenia

Najprościej: wykonaj kroki 1–6 z [szybkiego startu](#2-szybki-start), załaduj seed i przejdź [scenariusz](seeds/scenariusz-czesci-komputerowe.md).
Poniższa lista pozwala sprawdzić każdą funkcję osobno. Wynik oczekiwany podano w drugiej kolumnie.

### A. Logowanie i sesja

- [ ] Zaloguj się `admin` / hasło → **panel Zarządzanie**.
- [ ] Błędne hasło → komunikat **„Nieprawidłowy login, e-mail lub hasło."** (bez informacji, czy konto istnieje).
- [ ] Logowanie adresem e-mail zamiast loginu → działa.
- [ ] Wyloguj się z menu konta → powrót na stronę logowania.
- [ ] Ponad 10 prób logowania w minucie z jednego adresu → odmowa (limit zapytań).
- [ ] Wejdź bez logowania na `/main/management` → przekierowanie na logowanie.

### B. Katalog

- [ ] **Jednostka miary:** dodaj `Metr` (`m`) → pojawia się na liście. Dodaj drugą o tym samym symbolu → **„Jednostka o takiej nazwie lub symbolu już istnieje."**
- [ ] **Kategoria:** dodaj, edytuj opis, usuń pustą.
- [ ] **Produkt:** dodaj z kodem EAN i progami minimum/optimum → pojawia się na liście. Dodaj drugi z tym samym SKU → **„Produkt z takim SKU już istnieje."**
- [ ] Wyszukiwarka produktów filtruje po nazwie i SKU, a sortowanie po nagłówkach kolumn działa.

### C. Magazyny i użytkownicy

- [ ] Dodaj dwa magazyny i w każdym lokalizację `REC-01` (przyjęcia). Dodaj lokalizacje `A-01` i `PICK-01`.
- [ ] Dodaj użytkownika z rolą **Pracownik** przypisanego do magazynu. Rola **Administrator** nie wymaga magazynu, pozostałe tak.
- [ ] Zaloguj się jako pracownik → menu bez *Magazyny, Lokalizacje, Użytkownicy, Role, Audit log*, brak selektora magazynu.
- [ ] Jako pracownik wejdź ręcznie na `/main/management/users` → przekierowanie z powrotem do *Zarządzania*.

### D. Dokumenty magazynowe (jako administrator lub kierownik)

- [ ] **PW:** przyjmij 50 szt. produktu do `REC-01` → stan 50, w *Ruchach magazynowych* wpis „Przyjęcie wewnętrzne" ze zmianą +50. Numer dokumentu ma postać `ODDZIAŁ/PW/2026/000001`, np. `WARSZAWA/PW/2026/000001`.
- [ ] **RW** ponad stan (np. 9999) → **„Brak wystarczającej dostępnej ilości dla RW."**, stan bez zmian.
- [ ] **PW 0,5 szt.** produktu w jednostce `szt.` → **„Ilość musi być liczbą całkowitą dla tej jednostki miary."** Dla jednostki `m` ułamek jest dozwolony.
- [ ] **PW powyżej 1 000 000 szt.** → **„Ilość nie może przekraczać 1.000.000."**
- [ ] **PW nieaktywnego produktu** (wycofanego) → **„Dokument zawiera nieaktywny albo nieistniejący produkt."**
- [ ] **MM** z `REC-01` do `A-01` → stan w `REC-01` maleje, w `A-01` rośnie, suma bez zmian.
- [ ] **Inwentaryzacja** (ilość docelowa mniejsza o 2) → ruch typu „Inwentaryzacja" ze zmianą −2.

### E. Wysyłki między magazynami

- [ ] Aktywny magazyn A: *Wysyłki → Dodaj wysyłkę* → magazyn B → produkt (z listy lub **po kodzie EAN**) → ilość. Pozycja pokazuje „Dostępne: N".
- [ ] Po utworzeniu: stan w A pokazuje **rezerwację** tej ilości, a status to „Oczekuje na akceptację".
- [ ] **Anuluj** (przycisk w wierszu) → potwierdzenie w oknie → status „Anulowana", rezerwacja wraca do puli.
- [ ] Wysyłka do **tego samego magazynu** → **„Magazyn docelowy musi być inny niż źródłowy."**
- [ ] Ilość większa niż stan → **„Brak wystarczającej ilości dla kodu …"**
- [ ] **Akceptuj** → status „Zaakceptowana", rezerwacja nadal trzyma towar. **Spakuj** → „Spakowana" (od tej chwili dostępne **WZ** i **Naklejka**). **Wyślij** → „Wysłana", rezerwacja zamienia się w rozchód ze stanu A. „Anuluj" działa do momentu wysłania.
- [ ] **W drodze**, a potem w magazynie **B**: *Przyjmij wysyłkę* (wpisz numer albo zeskanuj), odhacz pozycje → status „Odebrana", towar w `REC-01` magazynu B.
- [ ] Próba przyjęcia w magazynie A → **„Brak dostępu do magazynu docelowego tej wysyłki."** Przyciski akceptacji i anulowania są widoczne tylko w magazynie źródłowym.
- [ ] **Historia** wysyłki pokazuje zdarzenia: utworzona, wysłana, w drodze, odebrana (pojawiają się po 2–3 sekundach).
- [ ] Dla wysyłki od statusu „Spakowana" (także „Wysłana", „W drodze", „Odebrana") w kolumnie *Dokumenty* są przyciski **WZ** i **Naklejka**, widoczne w każdym widoku wysyłek.

### F. Prośba o wysyłkę

- [ ] W magazynie B: *Prośba o wysyłkę* → produkt i ilość. System **sam wybiera magazyn źródłowy**: ten, który ma całe zapotrzebowanie i największą dostępność żądanych pozycji.
- [ ] Prośba, której nie pokrywa żaden magazyn → **„Nie znaleziono magazynu z pełnym zapotrzebowaniem dla tej prośby."**
- [ ] Prośba pojawia się w magazynie źródłowym jako wysyłka do akceptacji.

### G. Zamówienia zewnętrzne

- [ ] Dodaj dostawcę (*Kontrahenci*, typ: dostawca). *Zamówienia → Dodaj zamówienie* → dostawca, pozycje (cena domyślnie z katalogu).
- [ ] **Akceptuj** bez wypełnienia pól → **„Podaj numer faktury i numer dokumentu papierowego."** Wypełnij oba → status „Zaakceptowane".
- [ ] **Przyjmij**: przycisk „Przyjmij na stan" jest nieaktywny, dopóki nie odhaczysz pozycji na checkliście. Towar trafia do `REC-01`, a w *Ruchach* pojawia się „Przyjęcie od dostawcy".

### H. Sklep i pakowanie

- [ ] W sklepie (`http://localhost:8081`) dodaj produkt do koszyka, wypełnij dane i kup. Po zakupie pojawia się **token śledzenia** (64 znaki) i status „Nowe".
- [ ] W panelu (*Pakowanie*, magazyn, do którego trafiło zamówienie) → **Akceptuj** → **Spakuj** → towar schodzi ze stanu, ruch „Wydanie (WZ / sprzedaż)".
- [ ] Zamówienie z kilkoma różnymi produktami → przechodzi (trafia do jednego magazynu).
- [ ] Zamówienie na produkt bez stanu → **„Brak jednego magazynu, który może spakować całe zamówienie."**
- [ ] Zamówienie na ułamek sztuki → **„Ilość musi być liczbą całkowitą dla tej jednostki miary."**
- [ ] Klient wpisuje token w polu śledzenia → widzi status. **Anulowanie przez klienta** zwalnia rezerwację.
- [ ] Zamówienie, którego magazyn nie przyjmie do realizacji, **wygasa po 30 minutach** i jego rezerwacja wraca do puli (po przyjęciu termin to 24 godziny). Wygasanie sprawdza usługa w tle co kilkanaście sekund.

### I. Dashboard

- [ ] Kafelki: produkty na stanie, ilość łącznie, zarezerwowane, dostępne, aktywne rezerwacje, w drodze. Liczby zgadzają się ze *Stanami magazynowymi*.
- [ ] **Alerty:** produkt z ilością dostępną ≤ minimum → „Poniżej minimum", a < optimum → „Poniżej optimum".
- [ ] Sekcja **„Przepływ magazynu"** jest widoczna dla administratora i kierownika, a dla pracownika jej nie ma. Przełączniki 7 / 30 / 90 dni, wykres dzienny (przyjęcia, wydania, saldo), „Pokaż tabelę", najczęściej ruszane produkty, wysyłki w okresie.

### J. Audyt i uprawnienia

- [ ] *Audit log* pokazuje wpisy dla wszystkich powyższych operacji (utworzenie, akceptacja, przyjęcie, spakowanie) z użytkownikiem i opisem.
- [ ] Pracownik: statystyki przepływu i lista użytkowników dają odmowę (HTTP 403), a księgowanie PW nie jest dostępne.
- [ ] Administrator: *Zarządzanie → Role* pozwala zmienić uprawnienia roli (np. odebrać Pracownikowi `purchase-orders.create`). Zmiana działa **od razu**, bez ponownego logowania, więc zalogowany pracownik dostaje odmowę przy tworzeniu zamówienia. Po sprawdzeniu przywróć uprawnienie.

### K. Odporność

- [ ] Zatrzymaj i uruchom API ponownie → **wszystkie dane zostają** (start tylko migruje).
- [ ] Kolejne uruchomienie nie tworzy drugiego administratora.

---

## 6. Reguły, które warto znać

| Temat | Zachowanie |
|---|---|
| **Przyjęcia** | Zamówienie zewnętrzne i odbiór wysyłki wkładają towar do lokalizacji `REC-01` magazynu. Gdy jej brak, do pierwszej alfabetycznie. |
| **Rezerwacje** | Wysyłka (od utworzenia) i zamówienie ze sklepu rezerwują towar. Zarezerwowanego towaru nie da się przesunąć ani wydać innym dokumentem. |
| **Magazyn dla zamówienia ze sklepu** | Całe zamówienie musi się zmieścić w jednym magazynie. Wygrywa ten, który po realizacji zostaje z **najmniejszą nadwyżką** (a nie najbliższy). |
| **Źródło dla prośby o wysyłkę** | Magazyn z pełnym zapotrzebowaniem i największą dostępnością żądanych pozycji. |
| **Numery dokumentów** | `<ODDZIAŁ>/<TYP>/<rok>/<000001>` (nazwa oddziału bez polskich znaków, np. `WARSZAWA`), typy: `PW`, `RW`, `WZ` (wysyłka), `ZZ` (zamówienie zewnętrzne), `SW` (sklep), `REQ` (prośba). Numeracja jest osobna dla magazynu, typu i roku. |
| **Jednostki** | Ułamki ilości są dozwolone tylko dla jednostek ciągłych: `kg`, `g`, `t`, `m`, `mb`, `cm`, `mm`, `l`, `ml`, `m2`, `m3` (maks. 3 miejsca po przecinku, limit 1 000 000). |
| **Historia** | Zdarzenia stanów i wysyłek trafiają do osobnej bazy historii przez outbox, z opóźnieniem 2–3 s. |
| **Limity zapytań (na adres IP, na minutę)** | logowanie 10, zakupy w sklepie 20, śledzenie zamówienia 120. |
| **Statystyki przepływu** | Dni liczone w UTC. Wysyłki i przesunięcia MM liczą się jako wydanie i jako przyjęcie. |

## 7. Konfiguracja

| Klucz | Zmienna środowiskowa | Wymagany | Opis |
|---|---|:-:|---|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | tak | baza główna |
| `ConnectionStrings:HistoryConnection` | `ConnectionStrings__HistoryConnection` | tak | baza historii |
| `Jwt:SecretKey` | `Jwt__SecretKey` | tak | minimum 32 bajty, niezależny od środowiska DEV |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationMinutes` | `Jwt__Issuer` itd. | nie | domyślnie `MagazineAPI`, `MagazineAPI.Client`, 60 min (dozwolone 1–1440) |
| `InitialAdmin:Enabled` | `InitialAdmin__Enabled` | nie | `true` tworzy administratora, jeśli go nie ma |
| `InitialAdmin:Login`, `Password`, `Email`, `FirstName`, `LastName` | `InitialAdmin__…` | gdy `Enabled` | hasło minimum 8 znaków |

**CORS nie jest konfigurowany:** API dopuszcza zapytania z dowolnego adresu, nagłówka i metody. Dostęp chroni token JWT (nie ma ciasteczek), a nie adres strony.
Endpointy sklepu (`/api/Storefront/products`, składanie zamówienia, śledzenie i `sw.js`) są publiczne.

## 8. Przydatne polecenia

```powershell
# budowanie i lint
dotnet build MagazineAPI\MagazineAPI\MagazineAPI.csproj
cd Magazine-SPA; npm run build; npm run lint

# nowa migracja bazy głównej (z katalogu MagazineAPI\)
dotnet ef migrations add NazwaMigracji --context AppDbContext `
  -p MagazineAPIInfrastructure\MagazineAPInfrastructure.csproj -s MagazineAPI\MagazineAPI.csproj -o Migrations

# ręczne zastosowanie migracji na wskazanej bazie
dotnet ef database update --context AppDbContext --connection "<connection string>" `
  -p MagazineAPIInfrastructure\MagazineAPInfrastructure.csproj -s MagazineAPI\MagazineAPI.csproj
```

**Zaczynasz od zera?** Aplikacja nie ma żadnego automatycznego kasowania bazy. Usuń obie bazy (`DROP DATABASE Magazine; DROP DATABASE MagazineHistory;`),
uruchom API (utworzy schemat i administratora) i załaduj ponownie `seeds\seed-computer-parts-items.sql`.

## 9. Architektura w skrócie

```
MagazineAPI/
├── MagazineAPI/                 kontrolery HTTP, JWT i uprawnienia, usługi w tle, start aplikacji
├── MagazineAPIApplication/      logika: komendy i zapytania (CQRS, Mediator), walidacje, reguły ilości
├── MagazineAPIDomain/           encje, statusy, kody uprawnień (klucz główny każdej encji to `Id`)
├── MagazineAPIEvent/            zdarzenia i ich nasłuchiwacze (historia wysyłek i stanów)
└── MagazineAPIInfrastructure/   EF Core, mapowania, migracje, repozytoria, outbox historii
```

Panel (`Magazine-SPA/src/app/`) jest podzielony na moduły funkcjonalne, każdy z własnymi stronami, oknami, serwisami, modelami i trasami:

| Katalog | Adres | Zawartość |
|---|---|---|
| `inventory/` | `/main/inventory` | stany magazynowe, ruchy (`/movements`), dokumenty PW/RW/MM (`/operations`) |
| `shipments/` | `/main/shipments` | wysyłki między magazynami: wszystkie, oczekujące, pakowanie i wysyłka |
| `purchase-orders/` | `/main/purchase-orders` | zamówienia zewnętrzne |
| `storefront/` | `/main/storefront` | pakowanie (`/packing`) i zamówienia sklepu (`/orders`) |
| `management/` | `/main/management` | katalog, magazyny, lokalizacje, kontrahenci, użytkownicy, role, audyt |
| `dashboard/`, `auth/`, `core/`, `shared/` | | dashboard, logowanie, układ strony, wspólne komponenty i style |

Stare adresy `/main/management/shipments` itp. przekierowują na nowe.

- **CQRS:** kontroler wysyła komendę lub zapytanie przez `ISender`, a handler waliduje, zapisuje i publikuje zdarzenia.
- **Zdarzenia:** `StockMovementRecordedEvent` zapisuje wpis księgi ruchów w tej samej transakcji, a zdarzenia stanów i wysyłek trafiają do bazy historii przez outbox.
- **Transakcje:** operacje na stanach wykonują się w transakcji z ponawianiem, a przy składaniu zamówień w sklepie zamówienia kolejkują się na blokadzie, by uniknąć zakleszczeń.
- **Bazy:** główna (`AppDbContext`) i historii (`HistoryDbContext`), każda z własnymi migracjami.

## 10. Rozwiązywanie problemów

| Objaw | Przyczyna i rozwiązanie |
|---|---|
| API nie startuje: „Brak ConnectionStrings…" | uzupełnij oba connection stringi (krok 2) |
| API nie startuje: „Podaj niezależny sekret JWT…" | `Jwt:SecretKey` musi mieć co najmniej 32 bajty |
| API nie startuje: „Konfiguracja InitialAdmin musi zawierać komplet…" | przy `InitialAdmin.Enabled=true` podaj hasło (min. 8 znaków) |
| API nie startuje: tabela już istnieje | baza powstała starym trybem bez migracji. Usuń ją i uruchom API ponownie |
| Panel pokazuje „Network error" | API nie działa albo `apiUrl` w panelu wskazuje zły adres. CORS nie blokuje, bo dopuszcza wszystko |
| Panel nie loguje po zmianie portu API | popraw `apiUrl` w `Magazine-SPA/src/environments/environment.development.ts` |
| Sklep: „Nie udało się załadować sw.js" | API nie działa pod adresem z `StorefrontDemo/index.html` (domyślnie `localhost:5392`) |
| HTTP 429 | przekroczony limit zapytań na minutę, poczekaj minutę |
| `sqlcmd` zgłasza błąd indeksu (1934) | dodaj flagę `-I` |

## 11. Stan i ograniczenia

- **Brak testów automatycznych.** Poprawność sprawdza się ręcznie (lista w sekcji 5) i scenariuszem z `seeds/`.
- Dni w statystykach przepływu są liczone w UTC, więc ruchy tuż po północy czasu polskiego trafiają do sąsiedniego dnia.
- Aplikacja nie ma pojęcia kontenera ani magazynu przeładunkowego. Taką rolę pełni zwykły magazyn z zamówieniem zewnętrznym i wysyłkami.
