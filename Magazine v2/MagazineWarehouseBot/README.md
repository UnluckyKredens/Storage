# MagazineWarehouseBot

Osobny symulator ruchu magazynowego dla `MagazineAPI`.

## Start

1. Uruchom API na `http://localhost:5292`.
2. Uruchom bota:

```bash
dotnet run --project MagazineWarehouseBot/MagazineWarehouseBot.csproj
```

3. Otworz panel: `http://localhost:5260`.
4. Domyslnie bot loguje administratora `admin / Test123!`, tworzy kierownika i dwoch pracownikow dla kazdego oddzialu, a potem sam uruchamia symulacje. Panel pozwala nadpisac konta recznie.

Mozesz tez ustawic konto administratora zmiennymi srodowiskowymi:

```bash
BOT_ADMIN_LOGIN=... BOT_ADMIN_PASSWORD=... \
dotnet run --project MagazineWarehouseBot/MagazineWarehouseBot.csproj
```

Bot dziala przez publiczne endpointy API, wiec podlega tym samym uprawnieniom i walidacjom co uzytkownik w aplikacji.
Po starcie administrator przygotowuje zespol demo: po jednym kierowniku i dwoch pracownikow na oddzial. Kierownicy ksieguja PW/RW/MM, korekty, inwentaryzacje, akceptuja wysylki, oznaczaja transport i odbieraja checklisty. Pracownicy tworza wysylki oraz zapotrzebowania. Administrator moze wykonywac pelny zestaw akcji we wszystkich magazynach.
