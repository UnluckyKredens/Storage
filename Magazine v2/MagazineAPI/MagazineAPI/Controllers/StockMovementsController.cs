using MagazineAPI.Authorization;
using MagazineAPIApplication.Modules.StockMovements;
using MagazineAPIDomain.Authorization;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StockMovementsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionCodes.InventoryRead)]
    public async Task<ActionResult<IReadOnlyList<StockMovementView>>> GetAll(
        [FromQuery] Guid? productId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? sourceId,
        [FromQuery] int limit = 500,
        CancellationToken cancellationToken = default)
    {
        return Ok(await sender.Send(
            new GetStockMovementsQuery(productId, warehouseId, sourceId, limit),
            cancellationToken));
    }
}
