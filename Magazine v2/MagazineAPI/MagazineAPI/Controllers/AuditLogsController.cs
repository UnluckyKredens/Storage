using MagazineAPI.Authorization;
using MagazineAPIApplication.Modules.AuditLogs;
using MagazineAPIDomain.Authorization;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuditLogsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionCodes.RolesManage)]
    public async Task<ActionResult<IReadOnlyList<AuditLogView>>> GetAll(
        [FromQuery] string? entityName,
        [FromQuery] string? entityId,
        [FromQuery] int limit = 500,
        CancellationToken cancellationToken = default)
    {
        return Ok(await sender.Send(
            new GetAuditLogsQuery(entityName, entityId, limit),
            cancellationToken));
    }
}
