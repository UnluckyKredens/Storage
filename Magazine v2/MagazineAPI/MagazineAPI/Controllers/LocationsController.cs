using MagazineAPI.Authorization;
using MagazineAPIApplication.Modules.Locations;
using MagazineAPIApplication.Modules.Warehouses;
using MagazineAPIDomain.Authorization;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LocationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionCodes.WarehousesRead)]
    public async Task<ActionResult<IReadOnlyList<LocationView>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetLocationsQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionCodes.WarehousesRead)]
    public async Task<ActionResult<LocationView>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetLocationQuery(id), cancellationToken));
    }

    [HttpGet("page-data")]
    [HasPermission(PermissionCodes.WarehousesRead)]
    public async Task<ActionResult> PageData(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetWarehousesQuery(), cancellationToken));
    }

    [HttpPost]
    [HasPermission(PermissionCodes.WarehousesManage)]
    public async Task<ActionResult<LocationView>> Create([FromBody] SaveLocationCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = null }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionCodes.WarehousesManage)]
    public async Task<ActionResult<LocationView>> Update(Guid id, [FromBody] SaveLocationCommand command, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(command with { Id = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionCodes.WarehousesManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteLocationCommand(id), cancellationToken);
        return NoContent();
    }
}
