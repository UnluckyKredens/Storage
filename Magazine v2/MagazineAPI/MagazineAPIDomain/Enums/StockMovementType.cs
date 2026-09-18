namespace MagazineAPIDomain.Enums;

public enum StockMovementType
{
    ManualAdjustment = 1,
    TransferOut = 2,
    TransferIn = 3,
    Reservation = 4,
    ReservationRelease = 5,
    Correction = 6,
    InventoryCount = 7,
    InternalReceipt = 8,
    InternalIssue = 9,
    InternalTransferOut = 10,
    InternalTransferIn = 11
}
