using MagazineAPI.Authorization;
using MagazineAPIApplication.Modules.PurchaseOrders;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PurchaseOrdersController(ISender sender) : ControllerBase
{
    public sealed record ApprovePurchaseOrderRequest(
        string InvoiceNumber,
        string PaperDocumentNumber,
        string? Notes);

    public sealed record ReceivePurchaseOrderRequest(
        IReadOnlyCollection<Guid> CheckedItemIds,
        string? Notes);

    [HttpGet]
    [HasPermission(PermissionCodes.PurchaseOrdersRead)]
    public async Task<ActionResult<IReadOnlyList<PurchaseOrderView>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetPurchaseOrdersQuery(), cancellationToken));
    }

    [HttpGet("pending")]
    [HasPermission(PermissionCodes.PurchaseOrdersRead)]
    public async Task<ActionResult<IReadOnlyList<PurchaseOrderView>>> GetPending(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(
            new GetPurchaseOrdersQuery(PurchaseOrderStatus.PendingApproval),
            cancellationToken));
    }

    [HttpGet("approved")]
    [HasPermission(PermissionCodes.PurchaseOrdersRead)]
    public async Task<ActionResult<IReadOnlyList<PurchaseOrderView>>> GetApproved(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(
            new GetPurchaseOrdersQuery(PurchaseOrderStatus.Approved),
            cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionCodes.PurchaseOrdersRead)]
    public async Task<ActionResult<PurchaseOrderView>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetPurchaseOrderQuery(id), cancellationToken));
    }

    [HttpGet("page-data")]
    [HasPermission(PermissionCodes.PurchaseOrdersCreate)]
    public async Task<ActionResult<PurchaseOrderPageDataView>> PageData(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetPurchaseOrderPageDataQuery(), cancellationToken));
    }

    [HttpPost]
    [HasPermission(PermissionCodes.PurchaseOrdersCreate)]
    public async Task<ActionResult<PurchaseOrderView>> Create(
        [FromBody] CreatePurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, await sender.Send(new GetPurchaseOrderQuery(id), cancellationToken));
    }

    [HttpPost("{id:guid}/approve")]
    [HasPermission(PermissionCodes.PurchaseOrdersApprove)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApprovePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ApprovePurchaseOrderCommand(
            id,
            request.InvoiceNumber,
            request.PaperDocumentNumber,
            request.Notes), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/receive")]
    [HasPermission(PermissionCodes.PurchaseOrdersApprove)]
    public async Task<IActionResult> Receive(
        Guid id,
        [FromBody] ReceivePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ReceivePurchaseOrderCommand(id, request.CheckedItemIds, request.Notes), cancellationToken);
        return NoContent();
    }
}
