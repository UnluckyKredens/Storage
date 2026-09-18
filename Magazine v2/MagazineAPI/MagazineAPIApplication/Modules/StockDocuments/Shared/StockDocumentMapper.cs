using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;

namespace MagazineAPIApplication.Modules.StockDocuments;

public static class StockDocumentMapper
{
    public static StockDocumentListView ToListView(StockDocument document)
    {
        return new StockDocumentListView(
            document.Id,
            document.Number,
            document.Type,
            TypeName(document.Type),
            document.Status,
            StatusName(document.Status),
            document.WarehouseId,
            document.Warehouse.Name,
            document.ContractorId,
            document.Contractor?.Name,
            document.DestinationWarehouseId,
            document.DestinationWarehouse?.Name,
            document.CreatedAtUtc,
            document.CompletedAtUtc,
            document.ReceivedAtUtc,
            UserName(document.ReceivedByUser),
            UserName(document.ApprovedByUser),
            document.Items.Count);
    }

    public static StockDocumentDetailsView ToDetailsView(StockDocument document)
    {
        return new StockDocumentDetailsView(
            document.Id,
            document.Number,
            document.Type,
            TypeName(document.Type),
            document.Status,
            StatusName(document.Status),
            document.WarehouseId,
            document.Warehouse.Name,
            document.ContractorId,
            document.Contractor?.Name,
            document.DestinationWarehouseId,
            document.DestinationWarehouse?.Name,
            document.CreatedByUserId,
            UserName(document.CreatedByUser)!,
            document.ApprovedByUserId,
            UserName(document.ApprovedByUser),
            document.ReceivedByUserId,
            UserName(document.ReceivedByUser),
            document.CreatedAtUtc,
            document.CompletedAtUtc,
            document.ReceivedAtUtc,
            document.Notes,
            document.Items
                .OrderBy(item => item.Product.Name)
                .Select(item => new StockDocumentItemView(
                    item.Id,
                    item.ProductId,
                    item.Product.Name,
                    item.Product.Sku,
                    item.LocationId,
                    item.Location.LocationCode,
                    item.Quantity))
                .ToArray());
    }

    private static string TypeName(StockDocumentType type)
    {
        return type == StockDocumentType.Receipt ? "Przyjęcie PZ" : "Wysyłka WZ";
    }

    private static string StatusName(StockDocumentStatus status)
    {
        return status switch
        {
            StockDocumentStatus.Draft => "Szkic",
            StockDocumentStatus.Received => "Przyjęty — oczekuje na zatwierdzenie",
            _ => "Zatwierdzony"
        };
    }

    private static string? UserName(User? user)
    {
        return user is null ? null : $"{user.FirstName} {user.LastName}";
    }
}
