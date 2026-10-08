using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIApplication.Modules.Warehouses;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed class GetPurchaseOrderPageDataQueryHandler(
    IRepository<Warehouse> warehouseRepository,
    IRepository<Contractor> contractorRepository,
    IRepository<Product> productRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetPurchaseOrderPageDataQuery, PurchaseOrderPageDataView>
{
    public async ValueTask<PurchaseOrderPageDataView> Handle(
        GetPurchaseOrderPageDataQuery query,
        CancellationToken cancellationToken)
    {
        var warehouseId = warehouseContext.WarehouseId
            ?? throw new CommandValidationException("Wybierz magazyn dla zamówienia.");
        var warehouse = await warehouseRepository.FirstOrDefaultAsync(
            item => item.WarehouseId == warehouseId,
            cancellationToken)
            ?? throw new CommandValidationException("Magazyn nie istnieje.");

        var suppliers = await contractorRepository.Query()
            .Where(contractor => contractor.Type == ContractorType.Supplier ||
                                 contractor.Type == ContractorType.Both)
            .OrderBy(contractor => contractor.Name)
            .Select(contractor => new PurchaseOrderContractorView(
                contractor.ContractorId,
                contractor.Name,
                contractor.TaxNumber))
            .ToArrayAsync(cancellationToken);

        var products = await productRepository.Query()
            .Where(product => product.IsActive)
            .OrderBy(product => product.Name)
            .Select(product => new PurchaseOrderProductView(
                product.ProductId,
                product.Name,
                product.Sku,
                product.Barcode ?? product.Sku,
                product.PurchasePrice))
            .ToArrayAsync(cancellationToken);

        return new PurchaseOrderPageDataView(WarehouseMapper.ToView(warehouse), suppliers, products);
    }
}
