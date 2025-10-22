using MagazineAPI.Authorization;
using MagazineAPIApplication.Modules.Warehouses;
using MagazineAPIDomain.Authorization;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WarehousesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionCodes.WarehousesRead)]
    public async Task<ActionResult<IReadOnlyList<WarehouseView>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetWarehousesQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionCodes.WarehousesRead)]
    public async Task<ActionResult<WarehouseView>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetWarehouseQuery(id), cancellationToken));
    }

    [HttpPost]
    [HasPermission(PermissionCodes.WarehousesManage)]
    public async Task<ActionResult<WarehouseView>> Create([FromBody] SaveWarehouseCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = null }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionCodes.WarehousesManage)]
    public async Task<ActionResult<WarehouseView>> Update(Guid id, [FromBody] SaveWarehouseCommand command, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(command with { Id = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionCodes.WarehousesManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteWarehouseCommand(id), cancellationToken);
        return NoContent();
    }
}
