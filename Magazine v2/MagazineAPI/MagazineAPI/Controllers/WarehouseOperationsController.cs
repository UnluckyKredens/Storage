using MagazineAPI.Authorization;
using MagazineAPIApplication.Modules.WarehouseOperations;
using MagazineAPIDomain.Authorization;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WarehouseOperationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionCodes.InventoryRead)]
    public async Task<ActionResult<IReadOnlyList<WarehouseOperationView>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetWarehouseOperationsQuery(), cancellationToken));
    }

    [HttpPost("complete")]
    [HasPermission(PermissionCodes.InventoryManage)]
    public async Task<ActionResult> Complete(
        [FromBody] CompleteWarehouseOperationCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id }, new { id });
    }
}
