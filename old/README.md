# Storage

Prosty system **Storage** z backendem w **NestJS** i frontendem typu SPA w **Angular**. Repozytorium służyo do nauki NestJS – kod jest minimalistyczny, ale zawiera pełny przepływ: baza danych ⇄ API ⇄ SPA.

> **Uwaga**: Bazę danych tworzysz **samodzielnie** (instrukcje poniżej). Po skonfigurowaniu środowiska możesz zapełnić ją danymi wzorcowymi poleceniem `npm run seed` (szczegóły w sekcji *Seed*).

---

## Spis treści
1. [Wymagania](#wymagania)
2. [Struktura repozytorium](#struktura-repozytorium)
3. [Szybki start](#szybki-start)
4. [Konfiguracja bazy danych](#konfiguracja-bazy-danych)
5. [Backend – uruchomienie](#backend--uruchomienie)
6. [Seed – dane wzorcowe](#seed--dane-wzorcowe)
7. [Frontend – uruchomienie](#frontend--uruchomienie)
8. [Architektura i zasady](#architektura-i-zasady)
9. [Skrypty npm](#skrypty-npm)
10. [Troubleshooting](#troubleshooting)
---

## Wymagania
- **Node.js** (LTS) + **npm**
- **Angular CLI**
- **NestJS**  
- **MySQL 8.x** (lub kompatybilny – w razie innego silnika dostosuj konfigurację ORM)

---

## Struktura repozytorium
```
Storage/
├─ storage-api/         # Backend (NestJS)
├─ Storage-SPA/         # Frontend (SPA)
├─ Storage Schema.mwb   # Model bazy danych (MySQL Workbench)
└─ README.md
```

---

## Szybki start
```bash
git clone https://github.com/UnluckyKredens/Storage.git
cd Storage
```
1. **Utwórz własną bazę danych** (sekcja niżej).
2. **Skonfiguruj backend** (`storage-api/app.module.ts`).
3. **Uruchom backend**.
4. (Opcjonalnie) **Zasiej** dane: `npm run seed`.
5. **Uruchom frontend**.

---

## Konfiguracja bazy danych
> Poniżej przykład dla **MySQL**.

**SQL – utworzenie bazy i użytkownika**
```sql
CREATE DATABASE storage;
```

**Konfiguracja backendu (`storage-api/app.module.ts`)**
```ts
    TypeOrmModule.forRoot({
      type: 'mysql',
      host: 'localhost',
      port: 3306,
      username: '', //Type your own username
      password: '', //Type your own password
      database: 'storage',
      entities: [Item, Category, Stock, Purchase, Seller, Statistics],
      synchronize: true,
      autoLoadEntities: true,
    }),
```

> W repozytorium znajdziesz plik **`Storage Schema.mwb`** – to szkic schematu bazy w MySQL Workbench.

---

## Backend – uruchomienie
```bash
cd storage-api
npm install
npm run start:dev   # lub: nest start --watch
```
API powinno nasłuchiwać pod `http://localhost:3000` (o ile nie zmienisz `PORT`).

---

## Seed – dane wzorcowe
- **Polecenie**: `npm run seed`
- **Cel**: szybkie wstawienie przykładowych rekordów do świeżej bazy.
- **Skąd uruchamiać**: w katalogu `storage-api` po poprawnej konfiguracji `app.module.ts` i działającym połączeniu z DB.

---

## Frontend – uruchomienie
```bash
cd ../Storage-SPA
npm install
npm run start       # domyślnie np. http://localhost:4200
```
---

## Architektura i zasady
**Backend (NestJS)**
- **`AppModule`** jest modułem głównym, spina zależności.
- Warstwa dostępu do danych korzysta z ORM (konfiguracja przez `app.module.ts`).

**Przepływ danych**
1. SPA wywołuje endpointy REST backendu.
2. Backend wykonuje operacje na bazie (CRUD) i zwraca DTO do SPA.

**Konwencje**
- Nazewnictwo modułów/serwisów zgodne z NestJS.
- Zmienne środowiskowe w `app.module.ts`
---

## Skrypty npm
**Backend (`storage-api/`)**
- `npm run start` – start API
- `npm run start:dev` – start w trybie watch
- `npm run build` – (jeśli zdefiniowany) buduje kod TS
- `npm run seed` – wstawia dane wzorcowe do DB

**Frontend (`Storage-SPA/`)**
- `npm run start` – dev‑server SPA
- `npm run build` – build produkcyjny

> Dokładną listę skryptów znajdziesz w odpowiednich `package.json`.

---

## Troubleshooting
- **Brak połączenia z DB**: sprawdź `app.module.ts`, czy MySQL działa i czy użytkownik ma uprawnienia. Przetestuj połączenie klientem SQL.
- **Seed się wywraca**: upewnij się, że tabele istnieją (migracje / synchronize) i że schemat jest zgodny z oczekiwaniami serwisu seedującego.
- **Błędy CORS/HTTP w SPA**: sprawdź bazowy URL API w SPA oraz konfigurację CORS w backendzie.

---
