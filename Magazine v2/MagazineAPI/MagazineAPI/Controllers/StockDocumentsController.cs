using MagazineAPI.Authorization;
using MagazineAPIApplication.Common.ReadModels;
using MagazineAPIApplication.Modules.StockDocuments;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StockDocumentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionCodes.StockDocumentsRead)]
    public async Task<ActionResult<PaginationReadModel<StockDocumentListView>>> GetAll(
        [FromQuery] StockDocumentType type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string search = "",
        [FromQuery] string sortBy = "created",
        [FromQuery] string order = "desc",
        CancellationToken cancellationToken = default)
    {
        return Ok(await sender.Send(
            new GetStockDocumentsQuery(type, null, page, pageSize, search, sortBy, order),
            cancellationToken));
    }

    [HttpGet("history")]
    [HasPermission(PermissionCodes.StockDocumentsRead)]
    public async Task<ActionResult<PaginationReadModel<StockDocumentListView>>> GetHistory(
        [FromQuery] StockDocumentType? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string search = "",
        [FromQuery] string sortBy = "completed",
        [FromQuery] string order = "desc",
        CancellationToken cancellationToken = default)
    {
        return Ok(await sender.Send(
            new GetStockDocumentsQuery(
                type,
                StockDocumentStatus.Completed,
                page,
                pageSize,
                search,
                sortBy,
                order),
            cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionCodes.StockDocumentsRead)]
    public async Task<ActionResult<StockDocumentDetailsView>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetStockDocumentQuery(id), cancellationToken));
    }

    [HttpGet("page-data")]
    [HasPermission(PermissionCodes.StockDocumentsManage)]
    public async Task<ActionResult<StockDocumentPageDataView>> PageData(
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(
            new GetStockDocumentPageDataQuery(),
            cancellationToken));
    }

    [HttpGet("shipment-page-data")]
    [HasPermission(PermissionCodes.StockShipmentsCreate)]
    public async Task<ActionResult<StockDocumentPageDataView>> ShipmentPageData(
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(
            new GetStockDocumentPageDataQuery(),
            cancellationToken));
    }

    [HttpGet("scan/{id:guid}")]
    [HasPermission(PermissionCodes.StockDocumentsReceive)]
    public async Task<ActionResult<StockDocumentDetailsView>> Scan(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(
            new GetStockDocumentForReceivingQuery(id),
            cancellationToken));
    }

    [HttpPost("{id:guid}/receive")]
    [HasPermission(PermissionCodes.StockDocumentsReceive)]
    public async Task<ActionResult<StockDocumentDetailsView>> Receive(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ReceiveStockDocumentCommand(id), cancellationToken);
        return Ok(await sender.Send(
            new GetStockDocumentForReceivingQuery(id),
            cancellationToken));
    }

    [HttpPost]
    [HasPermission(PermissionCodes.StockDocumentsManage)]
    public async Task<ActionResult<StockDocumentDetailsView>> Create(
        [FromBody] SaveStockDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command with { Id = null }, cancellationToken);
        var result = await sender.Send(new GetStockDocumentQuery(id), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    [HttpPost("shipments")]
    [HasPermission(PermissionCodes.StockShipmentsCreate)]
    public async Task<ActionResult<StockDocumentDetailsView>> CreateShipment(
        [FromBody] SaveStockDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(
            command with
            {
                Id = null,
                Type = StockDocumentType.Shipment,
                ContractorId = null
            },
            cancellationToken);
        var result = await sender.Send(new GetStockDocumentQuery(id), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionCodes.StockDocumentsManage)]
    public async Task<ActionResult<StockDocumentDetailsView>> Update(
        Guid id,
        [FromBody] SaveStockDocumentCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command with { Id = id }, cancellationToken);
        return Ok(await sender.Send(new GetStockDocumentQuery(id), cancellationToken));
    }

    [HttpPost("{id:guid}/complete")]
    [HasPermission(PermissionCodes.StockDocumentsApprove)]
    public async Task<ActionResult<StockDocumentDetailsView>> Complete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(new CompleteStockDocumentCommand(id), cancellationToken);
        return Ok(await sender.Send(new GetStockDocumentQuery(id), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionCodes.StockDocumentsManage)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteStockDocumentCommand(id), cancellationToken);
        return NoContent();
    }
}
