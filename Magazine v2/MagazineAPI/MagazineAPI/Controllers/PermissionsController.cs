using MagazineAPI.Authorization;
using MagazineAPI.Contracts.Roles;
using MagazineAPIApplication.Modules.Permissions;
using MagazineAPIDomain.Authorization;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[HasPermission(PermissionCodes.RolesManage)]
[Route("api/[controller]")]
public sealed class PermissionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PermissionView>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetPermissionCatalogQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PermissionView>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetPermissionQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<PermissionView>> Create([FromBody] SavePermissionRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SavePermissionCommand(null, request.Code, request.Name, request.Description), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PermissionView>> Update(Guid id, [FromBody] SavePermissionRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new SavePermissionCommand(id, request.Code, request.Name, request.Description), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeletePermissionCommand(id), cancellationToken);
        return NoContent();
    }
}
