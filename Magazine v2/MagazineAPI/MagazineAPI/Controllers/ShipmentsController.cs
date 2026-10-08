using MagazineAPI.Authorization;
using MagazineAPIApplication.Modules.Shipments;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ShipmentsController(ISender sender) : ControllerBase
{
    public sealed record ReceiveShipmentRequest(IReadOnlyCollection<Guid> CheckedItemIds, string? Notes);

    [HttpGet]
    [HasPermission(PermissionCodes.ShipmentsRead)]
    public async Task<ActionResult<IReadOnlyList<ShipmentView>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetShipmentsQuery(), cancellationToken));
    }

    [HttpGet("ready")]
    [HasPermission(PermissionCodes.ShipmentsRead)]
    public async Task<ActionResult<IReadOnlyList<ShipmentView>>> GetReady(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetShipmentsQuery(ReadyOnly: true), cancellationToken));
    }

    [HttpGet("pending")]
    [HasPermission(PermissionCodes.ShipmentsRead)]
    public async Task<ActionResult<IReadOnlyList<ShipmentView>>> GetPending(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(
            new GetShipmentsQuery(Status: ShipmentStatus.PendingApproval),
            cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionCodes.ShipmentsRead)]
    public async Task<ActionResult<ShipmentView>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetShipmentQuery(id), cancellationToken));
    }

    [HttpGet("lookup")]
    [HasPermission(PermissionCodes.ShipmentsApprove)]
    public async Task<ActionResult<ShipmentView>> Lookup(
        [FromQuery] string identifier,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetShipmentByIdentifierQuery(identifier), cancellationToken));
    }

    [HttpGet("{id:guid}/history")]
    [HasPermission(PermissionCodes.ShipmentsRead)]
    public async Task<ActionResult<IReadOnlyList<ShipmentHistoryView>>> GetHistory(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetShipmentHistoryQuery(id), cancellationToken));
    }

    [HttpGet("page-data")]
    [HasPermission(PermissionCodes.ShipmentsCreate)]
    public async Task<ActionResult<ShipmentPageDataView>> PageData(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetShipmentPageDataQuery(), cancellationToken));
    }

    [HttpGet("product-by-barcode")]
    [HasPermission(PermissionCodes.ShipmentsCreate)]
    public async Task<ActionResult<ShipmentProductView>> ProductByBarcode(
        [FromQuery] string barcode,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetProductByBarcodeQuery(barcode), cancellationToken));
    }

    [HttpGet("product")]
    [HasPermission(PermissionCodes.ShipmentsCreate)]
    public async Task<ActionResult<ShipmentProductView>> Product(
        [FromQuery] Guid productId,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetProductForShipmentQuery(productId), cancellationToken));
    }

    [HttpPost]
    [HasPermission(PermissionCodes.ShipmentsCreate)]
    public async Task<ActionResult<ShipmentView>> Create(
        [FromBody] CreateShipmentCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, await sender.Send(new GetShipmentQuery(id), cancellationToken));
    }

    [HttpPost("request")]
    [HasPermission(PermissionCodes.ShipmentsCreate)]
    public async Task<ActionResult<ShipmentView>> CreateRequest(
        [FromBody] CreateShipmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, await sender.Send(new GetShipmentQuery(id), cancellationToken));
    }

    [HttpPost("{id:guid}/approve")]
    [HasPermission(PermissionCodes.ShipmentsApprove)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new ApproveShipmentCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/in-transit")]
    [HasPermission(PermissionCodes.ShipmentsApprove)]
    public async Task<IActionResult> MarkInTransit(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new MarkShipmentInTransitCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/receive")]
    [HasPermission(PermissionCodes.ShipmentsApprove)]
    public async Task<IActionResult> Receive(
        Guid id,
        [FromBody] ReceiveShipmentRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ReceiveShipmentCommand(id, request.CheckedItemIds, request.Notes), cancellationToken);
        return NoContent();
    }
}
