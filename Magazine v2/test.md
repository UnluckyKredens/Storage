# QA: test czystej instancji od deski do deski

## Cel

Dokument opisuje, jak tester QA ma sprawdzic gotowa, ale pusta instancje systemu magazynowego. Tester nie uruchamia bazy, nie seeduje danych technicznie, nie buduje projektu i nie wykonuje testow automatycznych. Tester sam tworzy wszystkie dane recznie przez interfejs aplikacji.

Pusta lista produktow, magazynow, lokalizacji, stanow albo dokumentow na poczatku testu jest poprawnym stanem systemu.

QA sprawdza:

1. Czy system pozwala utworzyc dane od zera.
2. Czy utworzone dane sa widoczne w kolejnych modulach.
3. Czy operacje magazynowe poprawnie zmieniaja statusy, stany i historie ruchow.
4. Czy aplikacja blokuje niepoprawne operacje.
5. Czy pelny proces dziala od utworzenia danych bazowych do zamowienia i pakowania.

## 1. Dostep do systemu

Tester powinien otrzymac:

1. Adres panelu administracyjnego.
2. Adres sklepu demo, jezeli jest testowany.
3. Login i haslo uzytkownika testowego.
4. Informacje, czy po tescie trzeba usunac dane testowe.

Przykladowe dane logowania, jezeli srodowisko korzysta z domyslnej konfiguracji:

```text
login: admin
haslo: Test123!
```

## 2. Zasady testowania

Wszystkie rekordy tworzone przez QA oznaczaj prefiksem `QA`, np.:

```text
QA Magazyn Glowny
QA Magazyn Pomocniczy
QA Produkt Testowy A
QA Produkt Testowy B
QA Dostawca Testowy
```

Podczas testow zwracaj uwage na:

1. Czy aplikacja dobrze obsluguje puste listy.
2. Czy po dodaniu pierwszego rekordu lista automatycznie sie odswieza.
3. Czy formularze blokuja brak wymaganych danych.
4. Czy po odswiezeniu strony utworzone dane nadal istnieja.
5. Czy dane utworzone w jednym module sa dostepne w innych modulach.
6. Czy statusy dokumentow zmieniaja sie w logicznej kolejnosci.
7. Czy stany magazynowe nie schodza ponizej zera.
8. Czy aplikacja nie pokazuje bledow typu `500`, `undefined`, `null` albo stack trace.
9. Czy uzytkownik widzi zrozumialy komunikat sukcesu albo bledu.
10. Czy dlugie nazwy i wieksze ilosci nie psuja ukladu strony.

Kazdy blad zapisuj z:

1. Krokiem, w ktorym wystapil.
2. Oczekiwanym wynikiem.
3. Rzeczywistym wynikiem.
4. Screenshotem.
5. Nazwa rekordu albo numerem dokumentu.

## 3. Test logowania

Kroki:

1. Otworz panel administracyjny.
2. Wpisz poprawny login i haslo.
3. Zaloguj sie.
4. Wyloguj sie.
5. Sprobuj zalogowac sie niepoprawnym haslem.

Oczekiwany wynik:

1. Poprawne dane loguja uzytkownika do panelu.
2. Po zalogowaniu widoczna jest nawigacja systemu.
3. Wylogowanie przenosi uzytkownika do ekranu logowania.
4. Niepoprawne dane nie loguja uzytkownika.
5. Komunikat bledu jest zrozumialy.

Na co zwrocic uwage:

1. Czy sesja nie wygasa od razu po zalogowaniu.
2. Czy odswiezenie strony nie wyrzuca uzytkownika bez powodu.
3. Czy po wylogowaniu nie mozna wrocic przyciskiem Wstecz do panelu.

## 4. Sprawdzenie pustej instancji

Kroki:

1. Wejdz kolejno w produkty, magazyny, lokalizacje, stany magazynowe, wysylki, zamowienia i ruchy magazynowe.
2. Sprawdz, czy puste listy wyswietlaja sie poprawnie.
3. Sprawdz, czy widoczne sa przyciski dodawania nowych rekordow.

Oczekiwany wynik:

1. Aplikacja nie pokazuje bledu na pustych listach.
2. Pusty stan jest opisany czytelnie albo tabela jest po prostu pusta.
3. Uzytkownik moze rozpoczac dodawanie danych.
4. Menu i nawigacja dzialaja mimo braku danych.

Na co zwrocic uwage:

1. Czy nie pojawia sie blad ladowania danych.
2. Czy puste tabele nie maja popsutego ukladu.
3. Czy aplikacja nie wymaga danych, ktorych nie da sie jeszcze utworzyc.

## 5. Utworzenie danych bazowych

Ta sekcja przygotowuje minimalny zestaw danych do dalszych testow. Jezeli aplikacja ma inne nazwy pol, wypelnij ich najblizsze odpowiedniki.

### 5.1. Utworz magazyny

Kroki:

1. Wejdz w modul magazynow.
2. Dodaj magazyn `QA Magazyn Glowny`.
3. Dodaj magazyn `QA Magazyn Pomocniczy`.
4. Odswiez liste.

Oczekiwany wynik:

1. Oba magazyny sa widoczne na liscie.
2. Po odswiezeniu strony nadal istnieja.
3. Szczegoly magazynu pokazuja poprawna nazwe i dane.

Na co zwrocic uwage:

1. Czy system wymaga wszystkich potrzebnych pol.
2. Czy nie mozna zapisac magazynu bez nazwy.
3. Czy komunikat po zapisie jest jasny.

### 5.2. Utworz lokalizacje

Kroki:

1. Wejdz w modul lokalizacji.
2. Dodaj lokalizacje `QA-A1` dla `QA Magazyn Glowny`.
3. Dodaj lokalizacje `QA-B1` dla `QA Magazyn Pomocniczy`.
4. Odswiez liste.

Oczekiwany wynik:

1. Lokalizacje sa widoczne na liscie.
2. Kazda lokalizacja jest przypisana do poprawnego magazynu.
3. Lokalizacje sa dostepne pozniej przy operacjach magazynowych, jezeli system ich wymaga.

Na co zwrocic uwage:

1. Czy lokalizacja nie moze zostac zapisana bez magazynu.
2. Czy kod lokalizacji jest walidowany.
3. Czy lista magazynow w formularzu zawiera magazyny utworzone przed chwila.

### 5.3. Utworz produkty

Kroki:

1. Wejdz w modul produktow.
2. Dodaj produkt `QA Produkt Testowy A`.
3. Dodaj produkt `QA Produkt Testowy B`.
4. Dla produktow uzupelnij wymagane pola, np. kod, nazwe, opis, cene, jednostke.
5. Odswiez liste.
6. Otworz szczegoly produktu.

Oczekiwany wynik:

1. Produkty sa widoczne na liscie.
2. Szczegoly produktu pokazuja te same dane co lista.
3. Produkty mozna wyszukac po nazwie albo kodzie.
4. Po odswiezeniu strony produkty nadal istnieja.

Na co zwrocic uwage:

1. Czy nie mozna zapisac produktu bez nazwy albo kodu.
2. Czy cena nie przyjmuje tekstu ani wartosci ujemnej.
3. Czy dluga nazwa produktu nie psuje tabeli.

### 5.4. Utworz dostawce albo dane kontrahenta

Kroki:

1. Wejdz w modul dostawcow, kontrahentow albo ustawien wymaganych przez zamowienie zewnetrzne.
2. Dodaj `QA Dostawca Testowy`.
3. Uzupelnij wymagane pola.
4. Odswiez liste.

Oczekiwany wynik:

1. Dostawca zapisuje sie poprawnie.
2. Dostawca jest widoczny na liscie.
3. Dostawca jest dostepny pozniej przy tworzeniu zamowienia zewnetrznego.

Na co zwrocic uwage:

1. Czy system jasno pokazuje, ktore pola sa wymagane.
2. Czy nie mozna zapisac pustego dostawcy.
3. Czy formularz nie pozwala zapisac przypadkowo niepoprawnych danych kontaktowych, jezeli takie pola istnieja.

## 6. Nadanie pierwszego stanu magazynowego

Na czystej instancji sklep i wysylki moga nie dzialac, dopoki produkt nie ma stanu. Najpierw trzeba recznie utworzyc albo przyjac stan.

Kroki:

1. Wejdz w stany magazynowe albo dokument przyjecia.
2. Wybierz `QA Produkt Testowy A`.
3. Wybierz `QA Magazyn Glowny`.
4. Wybierz lokalizacje `QA-A1`, jezeli system jej wymaga.
5. Nadaj ilosc startowa, np. `20`.
6. Zapisz.
7. Powtorz dla `QA Produkt Testowy B`, np. ilosc `10`.
8. Odswiez stany magazynowe.

Oczekiwany wynik:

1. Produkty maja dodatni stan magazynowy.
2. Stan jest przypisany do poprawnego magazynu.
3. Ilosc dostepna jest rowna albo mniejsza od ilosci calkowitej.
4. W historii ruchow widac przyjecie albo utworzenie stanu, jezeli system rejestruje taka operacje.

Na co zwrocic uwage:

1. Czy nie mozna wpisac ilosci `0`, ujemnej albo tekstu, jezeli system tego zabrania.
2. Czy zapis stanu aktualizuje liste bez recznego kombinowania.
3. Czy ten sam produkt moze miec oddzielny stan w roznych magazynach.

## 7. Test edycji danych podstawowych

Kroki:

1. Edytuj opis albo cene produktu `QA Produkt Testowy A`.
2. Edytuj dane `QA Magazyn Pomocniczy`.
3. Edytuj lokalizacje `QA-B1`.
4. Odswiez strone po kazdej zmianie.

Oczekiwany wynik:

1. Zmiany zapisuja sie poprawnie.
2. Dane po odswiezeniu sa aktualne.
3. Edycja danych podstawowych nie usuwa powiazan ze stanami albo dokumentami.

Na co zwrocic uwage:

1. Czy system nie pozwala usunac rekordu uzywanego w dokumencie bez ostrzezenia.
2. Czy zmiana nazwy produktu jest widoczna w listach wyboru.
3. Czy formularze zachowuja sie przewidywalnie po bledzie walidacji.

## 8. Test zamowienia zewnetrznego albo zakupowego

Kroki:

1. Wejdz w zamowienia zewnetrzne albo zakupowe.
2. Utworz nowe zamowienie `QA`.
3. Wybierz `QA Dostawca Testowy`.
4. Dodaj `QA Produkt Testowy A`.
5. Wpisz ilosc, np. `5`.
6. Zapisz dokument.
7. Zaakceptuj zamowienie.
8. Przyjmij towar do `QA Magazyn Glowny`.
9. Sprawdz stany magazynowe.
10. Sprawdz historie ruchow.

Oczekiwany wynik:

1. Zamowienie otrzymuje numer albo identyfikator.
2. Status dokumentu zmienia sie po kazdym etapie.
3. Po przyjeciu towaru stan produktu wzrasta o przyjeta ilosc.
4. W historii ruchow widac przyjecie.
5. Dokument zakonczony nie pozwala na przypadkowa edycje kluczowych danych.

Na co zwrocic uwage:

1. Czy nie mozna zapisac pustego zamowienia.
2. Czy nie mozna przyjac towaru bez pozycji.
3. Czy statusy nie przeskakuja w nielogicznej kolejnosci.
4. Czy przy odswiezeniu strony dokument zachowuje aktualny status.

## 9. Test wysylki miedzy magazynami

Kroki:

1. Wejdz w wysylki.
2. Utworz nowa wysylke `QA`.
3. Wybierz `QA Magazyn Glowny` jako magazyn zrodlowy.
4. Wybierz `QA Magazyn Pomocniczy` jako magazyn docelowy.
5. Dodaj `QA Produkt Testowy A`.
6. Wpisz ilosc mniejsza lub rowna dostepnej, np. `3`.
7. Zapisz wysylke.
8. Zaakceptuj wysylke.
9. Oznacz wysylke jako wyslana albo w transporcie, jezeli system ma taki etap.
10. Przyjmij wysylke w magazynie docelowym.
11. Sprawdz stany obu magazynow.
12. Sprawdz historie ruchow.

Oczekiwany wynik:

1. Wysylka zostaje utworzona z poprawnym statusem poczatkowym.
2. System blokuje wysylke wiekszej ilosci niz dostepna.
3. Po akceptacji albo wyslaniu stan magazynu zrodlowego jest odpowiednio zmniejszony lub zarezerwowany.
4. Po przyjeciu stan magazynu docelowego rosnie.
5. Ruchy magazynowe pokazuja zdjecie i przyjecie towaru.

Na co zwrocic uwage:

1. Czy magazyn zrodlowy i docelowy nie moga byc takie same.
2. Czy system nie pozwala przyjac wysylki dwa razy.
3. Czy po anulowaniu dokumentu stany wracaja do poprawnych wartosci, jezeli anulowanie jest dostepne.
4. Czy historia dokumentu jasno pokazuje, kto i kiedy wykonal akcje.

## 10. Test sklepu demo

Ten test wykonaj dopiero po utworzeniu produktow i stanow magazynowych. Na pustej instancji sklep moze nie miec czego pokazac.

Kroki:

1. Otworz sklep demo.
2. Sprawdz, czy widac produkty utworzone przez QA albo produkty dostepne do sprzedazy.
3. Otworz szczegoly produktu.
4. Dodaj produkt do koszyka.
5. Zmien ilosc w koszyku.
6. Wypelnij dane klienta testowego.
7. Zloz zamowienie.
8. Zapisz numer zamowienia.
9. Wejdz w tracking zamowienia.
10. Wpisz numer zamowienia i sprawdz status.

Oczekiwany wynik:

1. Produkty ze stanem sa widoczne w sklepie, jezeli system je publikuje.
2. Koszyk poprawnie liczy ilosci i wartosci.
3. Zamowienie zostaje utworzone.
4. Klient widzi numer zamowienia.
5. Tracking pokazuje poprawny status zamowienia.

Na co zwrocic uwage:

1. Czy sklep nie pokazuje produktow bez dostepnego stanu, jezeli nie powinien.
2. Czy koszyk nie pozwala zamowic wiecej niz dostepna ilosc.
3. Czy formularz klienta waliduje wymagane pola.
4. Czy tracking nie pokazuje danych innego zamowienia.

## 11. Test pakowania zamowienia sklepowego

Kroki:

1. Wroc do panelu administracyjnego.
2. Wejdz w modul zamowien do spakowania.
3. Znajdz zamowienie utworzone w sklepie demo.
4. Otworz szczegoly zamowienia.
5. Przyjmij zamowienie do realizacji.
6. Sprawdz pozycje zamowienia.
7. Oznacz zamowienie jako spakowane.
8. Sprawdz status zamowienia.
9. Sprawdz stany magazynowe.
10. Sprawdz historie ruchow.

Oczekiwany wynik:

1. Zamowienie ze sklepu pojawia sie w panelu.
2. Pozycje zamowienia sa zgodne z koszykiem.
3. Status zmienia sie po przyjeciu do realizacji.
4. Po spakowaniu status wskazuje zakonczenie albo gotowosc do wysylki.
5. Stan magazynowy zostaje pomniejszony.
6. W historii ruchow widac rozchod towaru.

Na co zwrocic uwage:

1. Czy zamowienie nie znika z systemu po zmianie statusu.
2. Czy spakowanie nie jest mozliwe bez pozycji.
3. Czy nie da sie spakowac tego samego zamowienia kilka razy.
4. Czy ilosci zdjete ze stanu odpowiadaja ilosciom z zamowienia.

## 12. Test walidacji i bledow

Sprawdz negatywne przypadki:

1. Zapis produktu bez wymaganej nazwy.
2. Zapis magazynu bez nazwy.
3. Zapis lokalizacji bez magazynu.
4. Zapis dokumentu bez pozycji.
5. Wpisanie ilosci `0`.
6. Wpisanie ilosci ujemnej.
7. Wpisanie tekstu w pole liczbowe.
8. Proba wyslania wiekszej ilosci niz dostepna.
9. Proba otwarcia nieistniejacego numeru zamowienia w trackingu.

Oczekiwany wynik:

1. System blokuje niepoprawna akcje.
2. Komunikat bledu jest zrozumialy.
3. Dane nie sa zapisywane czesciowo.
4. Aplikacja nie wyrzuca uzytkownika do pustej strony.
5. Nie pojawia sie blad typu `500`, `undefined`, `null`, stack trace albo surowa odpowiedz API.

## 13. Test spojnosci danych

Po wykonaniu pelnego procesu sprawdz:

1. Czy produkty utworzone w panelu sa dostepne w dokumentach.
2. Czy magazyny i lokalizacje utworzone w panelu sa dostepne w operacjach magazynowych.
3. Czy numer zamowienia w sklepie zgadza sie z numerem w panelu.
4. Czy produkty w zamowieniu sa takie same w sklepie i panelu.
5. Czy zmiana statusu w panelu jest widoczna w trackingu sklepu.
6. Czy stany magazynowe zmienily sie o dokladnie taka ilosc, jaka byla w dokumencie.
7. Czy historia ruchow zawiera wpis dla kazdej operacji magazynowej.
8. Czy po odswiezeniu strony dane sie nie cofaja.

## 14. Test interfejsu

Na kazdym glownym widoku sprawdz:

1. Czy pusty stan jest obsluzony poprawnie.
2. Czy tabele sa czytelne po dodaniu pierwszych rekordow.
3. Czy formularze maja podpisane pola.
4. Czy przyciski jasno wskazuja akcje.
5. Czy komunikaty sukcesu i bledu sa widoczne.
6. Czy ladowanie danych nie konczy sie pustym ekranem bez informacji.
7. Czy dluzsze nazwy produktow nie psuja ukladu.
8. Czy menu pozwala szybko przejsc do najwazniejszych modulow.
9. Czy aplikacja jest uzywalna po odswiezeniu strony.

## 15. Sciezka end-to-end do wykonania

Minimalny pelny test czystej instancji:

1. Zaloguj sie do panelu.
2. Potwierdz, ze listy moga byc puste i nie powoduja bledow.
3. Utworz dwa magazyny: glowny i pomocniczy.
4. Utworz po jednej lokalizacji dla kazdego magazynu.
5. Utworz dwa produkty testowe.
6. Utworz dostawce testowego, jezeli jest wymagany.
7. Nadaj pierwszy stan magazynowy produktom.
8. Potwierdz, ze produkty maja stan w magazynie glownym.
9. Utworz zamowienie zewnetrzne i przyjmij towar.
10. Potwierdz, ze stan produktu wzrosl.
11. Utworz wysylke miedzy magazynami.
12. Przyjmij wysylke w magazynie docelowym.
13. Potwierdz, ze stany obu magazynow sa poprawne.
14. Zloz zamowienie w sklepie demo.
15. Znajdz zamowienie w panelu.
16. Spakuj zamowienie.
17. Potwierdz, ze stan magazynowy spadl.
18. Sprawdz tracking zamowienia w sklepie.
19. Sprawdz historie ruchow magazynowych.
20. Wyloguj sie.

## 16. Kryteria zaliczenia testow

System mozna uznac za poprawnie przetestowany, jezeli:

1. Tester przeszedl od pustej instancji do pelnego procesu bez blokera.
2. Dane podstawowe zostaly utworzone recznie przez interfejs.
3. Utworzone produkty, magazyny, lokalizacje i dostawcy byly dostepne w kolejnych modulach.
4. Wszystkie utworzone dokumenty otrzymaly poprawne statusy.
5. Stany magazynowe zmienily sie zgodnie z wykonanymi operacjami.
6. Zamowienie ze sklepu bylo widoczne w panelu.
7. Tracking pokazal poprawny status zamowienia.
8. Historia ruchow potwierdzila przyjecia, przesuniecia i rozchody.
9. Formularze blokowaly podstawowe bledy uzytkownika.
10. Aplikacja nie pokazala krytycznych bledow technicznych.

## 17. Blad krytyczny

Za blad krytyczny uznaj:

1. Brak mozliwosci zalogowania sie.
2. Brak mozliwosci utworzenia pierwszego magazynu, produktu albo lokalizacji.
3. Brak mozliwosci nadania pierwszego stanu magazynowego.
4. Brak mozliwosci utworzenia dokumentu magazynowego.
5. Niepoprawne naliczanie stanow magazynowych.
6. Mozliwosc zejscia ze stanem ponizej zera.
7. Zgubienie zamowienia pomiedzy sklepem a panelem.
8. Brak zapisu historii ruchow.
9. Mozliwosc wykonania tej samej operacji konczacej kilka razy, np. ponowne spakowanie tego samego zamowienia.
10. Blad serwera przy podstawowej operacji uzytkownika.

## 18. Czego QA nie robi

QA nie wykonuje:

1. Technicznego seedowania bazy.
2. Uruchamiania kontenerow.
3. Budowania backendu.
4. Budowania frontendu.
5. Testow automatycznych.
6. Zmian konfiguracyjnych.

QA testuje gotowy system przez interfejs uzytkownika i sam tworzy dane potrzebne do pelnego scenariusza.
