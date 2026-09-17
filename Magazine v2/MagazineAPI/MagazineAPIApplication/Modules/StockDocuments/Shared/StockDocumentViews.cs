using MagazineAPIDomain.Enums;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed record StockDocumentListView(
    Guid Id,
    string Number,
    StockDocumentType Type,
    string TypeName,
    StockDocumentStatus Status,
    string StatusName,
    Guid WarehouseId,
    string WarehouseName,
    Guid? ContractorId,
    string? ContractorName,
    Guid? DestinationWarehouseId,
    string? DestinationWarehouseName,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime? ReceivedAtUtc,
    string? ReceivedBy,
    string? ApprovedBy,
    int ItemsCount);

public sealed record StockDocumentItemView(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Sku,
    Guid LocationId,
    string LocationCode,
    decimal Quantity);

public sealed record StockDocumentDetailsView(
    Guid Id,
    string Number,
    StockDocumentType Type,
    string TypeName,
    StockDocumentStatus Status,
    string StatusName,
    Guid WarehouseId,
    string WarehouseName,
    Guid? ContractorId,
    string? ContractorName,
    Guid? DestinationWarehouseId,
    string? DestinationWarehouseName,
    Guid CreatedByUserId,
    string CreatedBy,
    Guid? ApprovedByUserId,
    string? ApprovedBy,
    Guid? ReceivedByUserId,
    string? ReceivedBy,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime? ReceivedAtUtc,
    string? Notes,
    IReadOnlyList<StockDocumentItemView> Items);
