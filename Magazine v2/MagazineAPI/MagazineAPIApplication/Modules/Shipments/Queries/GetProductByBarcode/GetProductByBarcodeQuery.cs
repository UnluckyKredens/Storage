using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record GetProductByBarcodeQuery(string Barcode) : IQuery<ShipmentProductView>;
