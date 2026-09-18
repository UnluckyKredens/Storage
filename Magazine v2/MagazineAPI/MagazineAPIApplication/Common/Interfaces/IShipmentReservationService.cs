using MagazineAPIDomain.Entities;

namespace MagazineAPIApplication.Common.Interfaces;

public interface IShipmentReservationService
{
    Task ReserveAsync(
        Shipment shipment,
        IReadOnlyCollection<ShipmentItem> items,
        string note,
        CancellationToken cancellationToken = default);
}
