# Scenariusz: sieć magazynów części komputerowych

Cztery magazyny, jedna dostawa morska i pełny obieg: przyjęcie, rozdział, sprzedaż, uzupełnianie, korekty i raport kierownika.
Wszystkie liczby poniżej pochodzą z przebiegu kontrolnego tego scenariusza na czystej bazie, więc u Ciebie powinny wyjść takie same,
o ile zachowasz kolejność kroków.

## Fabuła

| Magazyn | Rola | Udział w dostawie |
|---|---|---|
| **Gdańsk – Port Przeładunkowy** | magazyn przeładunkowy przy porcie, przyjmuje kontenery morskie z Azji | zostawia ok. 10% bufora |
| **Kraków – Magazyn Główny** | główny magazyn i źródło uzupełnień dla pozostałych | ok. 55% |
| **Warszawa – Oddział** | oddział regionalny | ok. 20% |
| **Poznań – Oddział** | oddział regionalny | ok. 15% |

Aplikacja nie ma pojęcia „kontenera" ani „magazynu przeładunkowego". Gdańsk jest zwykłym magazynem, a jego rola wynika z tego, że
przyjmuje zamówienie zewnętrzne (kontener) i od razu rozsyła towar wysyłkami do pozostałych magazynów.

## 0. Przygotowanie

**Baza.** Uruchom API na pustej bazie (migracje wykonają się przy starcie), a potem załaduj katalog:

```powershell
sqlcmd -C -I -f 65001 -S <serwer> -d <baza> -U <użytkownik> -P "<hasło>" -i seeds\seed-computer-parts-items.sql
```

Seed dodaje **wyłącznie 13 kategorii i 87 przedmiotów** (w tym dwie pozycje wycofane). Nie tworzy magazynów, lokalizacji, stanów ani dokumentów,
więc wszystko poniżej robisz w aplikacji.

**Dane do utworzenia ręcznie** (Zarządzanie → Magazyny, Lokalizacje, Kontrahenci, Użytkownicy):

1. **Cztery magazyny** o nazwach z tabeli powyżej.
2. **W każdym magazynie cztery lokalizacje:** `REC-01` (przyjęcia), `A-01` (regał A), `PICK-01` (kompletacja), `RET-01` (zwroty i reklamacje).
   Lokalizacja `REC-01` jest obowiązkowa: przyjęcia z zamówień i z wysyłek trafiają właśnie do niej (gdy jej brak, do pierwszej alfabetycznie).
3. **Kontrahent-dostawca** (typ: dostawca), np. `Shenzhen Components Ltd.`.
4. **Użytkownicy:** kierownik w każdym magazynie (np. `kier.gda`, `kier.krk`, `kier.waw`, `kier.poz`) i pracownik w Krakowie (`prac.krk`).
   Ustaw własne hasła. Konto administratora zakłada się samo przy pierwszym starcie.

Kroki 1–7 wykonuj jako administrator, przełączając aktywny magazyn selektorem w lewym dolnym rogu. Operacje wysyłkowe dotyczą
magazynu **źródłowego**, a przyjęcia wysyłki magazynu **docelowego**.

## 1. Kontener z Azji trafia do Gdańska

Aktywny magazyn: **Gdańsk**. Zamówienia → Dodaj zamówienie → dostawca z kroku 0, 12 pozycji (cena zakupu zostaje domyślna z katalogu):

| SKU | Przedmiot | Ilość |
|---|---|---:|
| `CZ-CPU-R5-7600` | Procesor AMD Ryzen 5 7600 AM5 | 200 |
| `CZ-CPU-I5-14400F` | Procesor Intel Core i5-14400F LGA1700 | 200 |
| `CZ-GPU-RTX4060` | Karta graficzna GeForce RTX 4060 8 GB | 150 |
| `CZ-GPU-RX7600` | Karta graficzna Radeon RX 7600 8 GB | 100 |
| `CZ-GPU-RTX4070S` | Karta graficzna GeForce RTX 4070 SUPER 12 GB | 60 |
| `CZ-MB-B650` | Płyta główna B650 ATX AM5 | 100 |
| `CZ-MB-B760` | Płyta główna B760 ATX LGA1700 DDR5 | 100 |
| `CZ-RAM-D5-16` | Pamięć RAM DDR5 16 GB 5600 MHz | 600 |
| `CZ-SSD-1T` | Dysk SSD NVMe 1 TB Gen4 | 400 |
| `CZ-SSD-2T` | Dysk SSD NVMe 2 TB Gen4 | 200 |
| `CZ-PSU-650G` | Zasilacz ATX 650 W 80 Plus Gold | 150 |
| `CZ-CASE-ATX` | Obudowa Midi Tower ATX airflow | 100 |

- Wartość zamówienia: **1 219 600,00 zł**, status „Oczekuje na akceptację".
- **Akceptuj:** podaj numer faktury (np. `FV/2026/10/0417`) i numer dokumentu papierowego (np. `CMR-CONT-8841/GDA`). Pola są obowiązkowe.
- **Przyjmij:** odhacz wszystkie pozycje na checkliście → „Przyjmij na stan".
- Efekt: w Gdańsku 12 pozycji w lokalizacji `REC-01`, razem 2360 szt., a w Ruchach magazynowych 12 wpisów typu „Przyjęcie od dostawcy".

## 2. Rozdział z Gdańska do trzech magazynów

Aktywny magazyn: **Gdańsk**. Wysyłki → Dodaj wysyłkę → magazyn docelowy → pozycje (z listy albo po kodzie EAN).
Trzy wysyłki po 12 pozycji:

| SKU | Kraków | Warszawa | Poznań | zostaje w Gdańsku |
|---|---:|---:|---:|---:|
| `CZ-CPU-R5-7600` | 110 | 40 | 30 | 20 |
| `CZ-CPU-I5-14400F` | 110 | 40 | 30 | 20 |
| `CZ-GPU-RTX4060` | 80 | 30 | 25 | 15 |
| `CZ-GPU-RX7600` | 55 | 20 | 15 | 10 |
| `CZ-GPU-RTX4070S` | 30 | 14 | 10 | 6 |
| `CZ-MB-B650` | 55 | 20 | 15 | 10 |
| `CZ-MB-B760` | 55 | 20 | 15 | 10 |
| `CZ-RAM-D5-16` | 330 | 120 | 90 | 60 |
| `CZ-SSD-1T` | 220 | 80 | 60 | 40 |
| `CZ-SSD-2T` | 110 | 40 | 30 | 20 |
| `CZ-PSU-650G` | 80 | 30 | 25 | 15 |
| `CZ-CASE-ATX` | 55 | 20 | 15 | 10 |

**Co obserwować:**

1. Po utworzeniu wysyłki towar jest **zarezerwowany** w Gdańsku (np. `CZ-RAM-D5-16`: 600 szt., w tym 540 zarezerwowanych dla trzech wysyłek).
2. **Anulowanie:** utwórz czwartą wysyłkę (10 × `CZ-RAM-D5-16` do Warszawy) i anuluj ją przyciskiem „Anuluj" przed akceptacją.
   Rezerwacja wraca do puli (550 → 540 zarezerwowanych). Po akceptacji wysyłki anulować już nie można.
3. Dla każdej z trzech wysyłek: **Akceptuj** (rozchód ze stanu Gdańska) → **W drodze**.
4. Przełącz aktywny magazyn na docelowy i **Przyjmij wysyłkę** (wpisz numer lub zeskanuj, odhacz pozycje). Przyjęcie zadziała tylko
   w magazynie docelowym, a w źródłowym dostaniesz „Brak dostępu do magazynu docelowego tej wysyłki".
5. Historia wysyłki (przycisk „Historia") pokaże zdarzenia: utworzona, wysłana, w drodze, odebrana.

Stan po kroku 2: Gdańsk ma bufor z ostatniej kolumny, a pozostałe magazyny ilości z odpowiednich kolumn.

## 3. Sprzedaż online

Sklep (`StorefrontDemo` albo `sw.js`) składa zamówienia bez logowania:

| Zamówienie klienta | Wynik |
|---|---|
| 1 × `CZ-GPU-RTX4060` + 1 × `CZ-RAM-D5-16` | trafia do **Gdańska** |
| 40 × `CZ-GPU-RTX4060` | trafia do **Krakowa** (jedyny magazyn z takim stanem) |
| 1 × `CZ-GPU-RTX4090` | błąd: „Brak jednego magazynu, który może spakować całe zamówienie." (brak stanu) |
| 1 × `CZ-GPU-RTX4060` + 1 × `CZ-GPU-RTX4070S` | trafia do **Gdańska**, a klient je potem **anuluje** (rezerwacja wraca) |

**Reguła wyboru magazynu** (warto ją pokazać, bo jest nieoczywista): zamówienie musi być spakowane w całości z jednego magazynu,
a wygrywa ten, który po realizacji zostaje z **najmniejszą nadwyżką**, a nie najbliższy ani największy. Dlatego mały bufor w Gdańsku
„wyprzedaje się" pierwszy.

Pakowanie: aktywny magazyn Gdańsk → Pakowanie → zamówienie → **Akceptuj** → **Spakuj**. Po spakowaniu `CZ-GPU-RTX4060` w Gdańsku spada
do 14, a `CZ-RAM-D5-16` do 59. Klient widzi status po tokenie śledzenia (64 znaki, który dostaje po zakupie).

## 4. Alerty i uzupełnianie magazynu

Dashboard (jako administrator lub kierownik) pokazuje alerty per magazyn. Reguła: dostępna ilość ≤ minimum to **„Poniżej minimum"**,
a mniejsza niż optimum to **„Poniżej optimum"**.

| Magazyn | Alerty po wykonaniu całego scenariusza |
|---|---|
| Gdańsk | **9 × poniżej minimum** i 3 × poniżej optimum (bufor 10% jest mniejszy od progów, np. `CZ-GPU-RTX4070S` = 6 przy minimum 6) |
| Kraków | 3 × poniżej optimum |
| Warszawa, Poznań | po 12 × poniżej optimum |

Gdańsk z alertami „poniżej minimum" to naturalny sygnał do zamówienia kolejnego kontenera (krok 1 powtórzony).

**Prośba o wysyłkę** (Wysyłki → Prośba o wysyłkę), aktywny magazyn **Poznań**: 8 × `CZ-GPU-RTX4070S` i 10 × `CZ-GPU-RTX4060`.
System sam wybiera źródło, bo jest nim magazyn, który ma **pełne zapotrzebowanie** i największą dostępność żądanych pozycji, czyli **Kraków**.
Prośba pojawia się w Krakowie jako wysyłka „Oczekuje na akceptację". Zaakceptuj ją w Krakowie, oznacz „W drodze" i przyjmij w Poznaniu.
W Poznaniu `CZ-GPU-RTX4070S` rośnie z 10 do 18, a `CZ-GPU-RTX4060` z 25 do 35.

Prośba bez pokrycia: 40 × `CZ-GPU-RTX4070S` daje błąd „Nie znaleziono magazynu z pełnym zapotrzebowaniem dla tej prośby."

## 5. Operacje wewnętrzne w Krakowie

Aktywny magazyn **Kraków**, Dokumenty PW/RW/MM:

1. **MM (przesunięcie)** `CZ-GPU-RTX4060` z `REC-01` do `A-01`, 60 szt. → błąd „Brak wystarczającej dostępnej ilości dla MM.".
   W `REC-01` leży 70 szt., ale 40 jest zarezerwowane pod zamówienie ze sklepu, więc wolne jest tylko 30. **Zarezerwowanego towaru nie da się przesuwać.**
2. **MM** 20 szt. z `REC-01` do `A-01`, potem **MM** 10 szt. z `REC-01` do `PICK-01`. Teraz: `REC-01` = 40 (zarezerwowane), `A-01` = 20, `PICK-01` = 10.
3. **RW (rozchód)** 2 × `CZ-GPU-RTX4070S` z `REC-01` z notatką „Uszkodzone w transporcie". Stan w Krakowie 22 → 20.
4. **Inwentaryzacja** `CZ-SSD-1T` w `REC-01`: system ma 220, w rzeczywistości 218, więc wpisz ilość docelową 218.
   Ruch typu „Inwentaryzacja" ma zmianę −2.
5. **PW (przyjęcie)** w **Gdańsku**: zwrot od klienta, 1 × `CZ-GPU-RTX4060` do lokalizacji `RET-01`, notatka „Zwrot od klienta".

## 6. Walidacje (każda ma zakończyć się komunikatem, bez zmiany stanu)

| Działanie | Oczekiwany komunikat |
|---|---|
| RW 9999 × `CZ-RAM-D5-16` w Krakowie | Brak wystarczającej dostępnej ilości dla RW. |
| PW 0,5 szt. `CZ-RAM-D5-16` | Ilość musi być liczbą całkowitą dla tej jednostki miary. |
| PW wycofanego `CZ-GPU-GTX1660S` | Dokument zawiera nieaktywny albo nieistniejący produkt. |
| Wysyłka Kraków → Kraków | Magazyn docelowy musi być inny niż źródłowy. |
| Wysyłka 500 × `CZ-CPU-R5-7600` z Krakowa | Brak wystarczającej ilości dla kodu … |
| Nowy produkt z istniejącym SKU `CZ-CPU-R5-7600` | Produkt z takim SKU już istnieje. |

**Uprawnienia** (zaloguj się jako `prac.krk`): statystyki przepływu, lista użytkowników i księgowanie PW kończą się odmową (403).
Kierownik (`kier.krk`) widzi statystyki przepływu, ale nie widzi listy użytkowników.

## 7. Raport kierownika

Dashboard → sekcja „Przepływ magazynu" (7 dni) po wykonaniu kroków 1–5:

| Magazyn | Przyjęto | Wydano | Ruchów |
|---|---:|---:|---:|
| Gdańsk | 2361 | 2126 | 51 |
| Kraków | 1320 | 52 | 20 |
| Warszawa | 474 | 0 | 12 |
| Poznań | 378 | 0 | 14 |

Liczby wynikają z obiegu: Gdańsk przyjął 2360 szt. z kontenera i 1 zwrot, a wydał 2124 szt. wysyłkami i 2 szt. sprzedaży. Wysyłki między
magazynami oraz przesunięcia MM liczą się zarówno jako wydanie, jak i jako przyjęcie (stąd 52 w Krakowie: 18 szt. do Poznania, 2 RW,
2 z inwentaryzacji i 30 szt. przesunięć wewnętrznych).
Panel „Wysyłki w okresie" w Gdańsku pokazuje 4 utworzone, 3 wysłane, 3 odebrane i 1 anulowaną. Dziennik audytu ma po całym scenariuszu 32 wpisy.

## 8. Stan końcowy wybranych przedmiotów

| SKU | Gdańsk | Kraków | Warszawa | Poznań |
|---|---:|---:|---:|---:|
| `CZ-GPU-RTX4060` | 15 | 70 (w tym 40 zarezerwowane) | 30 | 35 |
| `CZ-GPU-RTX4070S` | 6 | 20 | 14 | 18 |
| `CZ-RAM-D5-16` | 59 | 330 | 120 | 90 |
| `CZ-SSD-1T` | 40 | 218 | 80 | 60 |

## Co ten scenariusz pokazuje

- **Pełny obieg towaru:** zamówienie zewnętrzne → przyjęcie → wysyłki między magazynami → sprzedaż → zwrot.
- **Rezerwacje:** towar zarezerwowany pod wysyłkę lub zamówienie znika z puli dostępnej, a anulowanie ją przywraca.
- **Automatyka:** wybór magazynu przez sklep (najmniejsza nadwyżka) i przez prośbę o wysyłkę (największa dostępność).
- **Kontrola:** progi minimum i optimum, walidacje ilości, uprawnienia ról, audyt i historia wysyłek.

## Jak zacząć od nowa

Aplikacja nie ma już flagi kasującej bazę przy starcie. Aby wrócić do stanu wyjściowego, usuń bazę główną i bazę historii (np. `DROP DATABASE`),
uruchom API (migracje utworzą schemat i konto administratora) i załaduj ponownie `seeds/seed-computer-parts-items.sql`.
