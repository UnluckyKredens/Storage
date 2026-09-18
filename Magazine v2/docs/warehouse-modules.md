# Moduly magazynowe - przewodnik dla developera

Ten dokument opisuje najwazniejsze granice modulow po stronie API i SPA. Jesli zmieniasz logike magazynu, najpierw znajdz modul odpowiedzialny za dany proces i unikaj kopiowania reguly biznesowej w kilku handlerach.

## API

### StockMovement

Pelna historia zmian ilosci i rezerwacji. Kazda operacja, ktora zmienia `Inventory.Quantity` albo `Inventory.ReservedQuantity`, powinna zapisac wpis przez `IStockMovementWriter`.

Najwazniejsze pliki:

- `MagazineAPIApplication/Modules/StockMovements`
- `MagazineAPIDomain/Entities/StockMovement.cs`
- `MagazineAPI/Services/StockMovementWriter.cs`

### AuditLog

Log decyzji biznesowych: utworzenie dokumentu, wyslanie, odbior, korekta, zakonczenie operacji. Nie zastepuje `StockMovement`, tylko tlumaczy kto i dlaczego wykonal akcje.

Najwazniejsze pliki:

- `MagazineAPIApplication/Modules/AuditLogs`
- `MagazineAPIDomain/Entities/AuditLog.cs`
- `MagazineAPI/Services/AuditLogWriter.cs`

### Numeracja dokumentow

Numery dokumentow sa generowane per magazyn, typ dokumentu i okres. Do generowania zawsze uzywaj `IDocumentNumberGenerator`.

Najwazniejsze pliki:

- `MagazineAPIDomain/Entities/DocumentNumberSequence.cs`
- `MagazineAPI/Services/DocumentNumberGenerator.cs`

### Wysylki i rezerwacje

Tworzenie wysylki lub zapotrzebowania rezerwuje towar przez `IShipmentReservationService`. Zatwierdzenie wysylki zuzywa aktywne rezerwacje i zmienia status na `Sent`. Statusy przeplywu:

1. `PendingApproval`
2. `Sent`
3. `InTransit`
4. `Received`

Najwazniejsze pliki:

- `MagazineAPIApplication/Modules/Shipments`
- `MagazineAPI/Services/ShipmentReservationService.cs`
- `MagazineAPIDomain/Entities/StockReservation.cs`

### Dokumenty PW/RW/MM, korekty i inwentaryzacja

Wspolny endpoint `POST /api/WarehouseOperations/complete` obsluguje typy:

- `InternalReceipt` - PW
- `InternalIssue` - RW
- `InternalTransfer` - MM
- `Correction` - korekta
- `InventoryCount` - inwentaryzacja

Najwazniejsze pliki:

- `MagazineAPIApplication/Modules/WarehouseOperations`
- `MagazineAPIDomain/Entities/WarehouseOperation.cs`
- `MagazineAPIDomain/Entities/WarehouseOperationItem.cs`

### Dashboard magazynowy

Dashboard jest osobnym modulem API i SPA. Dane bazuja na stanach, rezerwacjach, wysylkach i progach produktu `MinimumQuantity` oraz `OptimumQuantity`.

Najwazniejsze pliki:

- `MagazineAPIApplication/Modules/WarehouseDashboard`
- `MagazineAPI/Controllers/WarehouseDashboardController.cs`

## SPA

### Dashboard

Dashboard jest osobnym feature modulem pod sciezka `/main/dashboard` i ma osobny link w navbarze.

Najwazniejsze pliki:

- `Magazine-SPA/src/app/dashboard/dashboard.routes.ts`
- `Magazine-SPA/src/app/dashboard/pages/warehouse-dashboard`
- `Magazine-SPA/src/app/dashboard/services/warehouse-dashboard.service.ts`

### Zarzadzanie

Modul `management` zawiera kartoteki, dokumenty magazynowe, wysylki, ruchy magazynowe i audit log. Dashboard nie powinien wracac do `management`, chyba ze bedzie to tylko link pomocniczy.

Najwazniejsze pliki:

- `Magazine-SPA/src/app/management/management.routes.ts`
- `Magazine-SPA/src/app/management/containers/management-container`
- `Magazine-SPA/src/app/management/pages/warehouse-operations`

## Zasady zmian

- Zmiana ilosci lub rezerwacji musi zapisac `StockMovement`.
- Decyzja biznesowa uzytkownika powinna zapisac `AuditLog`.
- Nie generuj numeru dokumentu recznie; uzyj `IDocumentNumberGenerator`.
- Nie kopiuj logiki rezerwacji wysylki; uzyj `IShipmentReservationService`.
- Nowe widoki operacyjne dodawaj jako osobne feature modules, jesli maja byc w navbarze.
