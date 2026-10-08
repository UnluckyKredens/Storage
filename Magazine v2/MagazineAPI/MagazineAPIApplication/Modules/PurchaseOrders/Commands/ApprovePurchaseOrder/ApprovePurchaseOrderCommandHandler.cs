using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed class ApprovePurchaseOrderCommandHandler(
    IRepository<PurchaseOrder> orderRepository,
    IWarehouseContext warehouseContext,
    ICurrentUser currentUser,
    IAuditLogWriter auditLogWriter) : ICommandHandler<ApprovePurchaseOrderCommand, bool>
{
    public async ValueTask<bool> Handle(ApprovePurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.FirstOrDefaultAsync(
            item => item.PurchaseOrderId == command.PurchaseOrderId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Zamówienie zewnętrzne nie zostało znalezione.");

        if (order.Status != PurchaseOrderStatus.PendingApproval)
            throw new CommandValidationException("Tylko zamówienie oczekujące może zostać zaakceptowane.");
        if (!warehouseContext.CanAccess(order.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu zamówienia.");
        if (string.IsNullOrWhiteSpace(command.InvoiceNumber))
            throw new CommandValidationException("Podaj numer faktury.");
        if (string.IsNullOrWhiteSpace(command.PaperDocumentNumber))
            throw new CommandValidationException("Podaj numer dokumentu papierowego.");

        order.Status = PurchaseOrderStatus.Approved;
        order.ApprovedByUserId = currentUser.UserId;
        order.ApprovedOnUtc = DateTime.UtcNow;
        order.InvoiceNumber = command.InvoiceNumber.Trim();
        order.PaperDocumentNumber = command.PaperDocumentNumber.Trim();
        order.Notes = command.Notes?.Trim() ?? order.Notes;
        await orderRepository.UpdateAsync(order, cancellationToken);

        await auditLogWriter.RecordAsync(
            new AuditLogRecord(
                "Approve",
                nameof(PurchaseOrder),
                order.PurchaseOrderId.ToString(),
                $"Zaakceptowano zamówienie {order.Number}, faktura {order.InvoiceNumber}, dokument {order.PaperDocumentNumber}."),
            cancellationToken);

        return true;
    }
}
