using MagazineAPI.Authorization;
using MagazineAPIApplication.Modules.Inventories;
using MagazineAPIDomain.Authorization;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InventoryController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionCodes.InventoryRead)]
    public async Task<ActionResult<IReadOnlyList<InventoryView>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetInventoriesQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionCodes.InventoryRead)]
    public async Task<ActionResult<InventoryView>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetInventoryQuery(id), cancellationToken));
    }

    [HttpGet("page-data")]
    [HasPermission(PermissionCodes.InventoryRead)]
    public async Task<ActionResult<InventoryPageDataView>> PageData(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetInventoryPageDataQuery(), cancellationToken));
    }

    [HttpPost]
    [HasPermission(PermissionCodes.InventoryManage)]
    public async Task<ActionResult<InventoryView>> Create([FromBody] SaveInventoryCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command with { Id = null }, cancellationToken);
        var result = await sender.Send(new GetInventoryQuery(id), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionCodes.InventoryManage)]
    public async Task<ActionResult<InventoryView>> Update(Guid id, [FromBody] SaveInventoryCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command with { Id = id }, cancellationToken);
        return Ok(await sender.Send(new GetInventoryQuery(id), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionCodes.InventoryManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteInventoryCommand(id), cancellationToken);
        return NoContent();
    }
}
