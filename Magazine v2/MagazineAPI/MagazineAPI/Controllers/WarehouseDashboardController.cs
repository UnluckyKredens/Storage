using MagazineAPI.Authorization;
using MagazineAPIApplication.Modules.WarehouseDashboard;
using MagazineAPIDomain.Authorization;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WarehouseDashboardController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionCodes.InventoryRead)]
    public async Task<ActionResult<WarehouseDashboardView>> Get(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetWarehouseDashboardQuery(), cancellationToken));
    }
}
