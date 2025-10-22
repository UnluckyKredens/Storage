using MagazineAPI.Authorization;
using MagazineAPI.Contracts.Roles;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Modules.Roles;
using MagazineAPIDomain.Authorization;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[HasPermission(PermissionCodes.RolesManage)]
[Route("api/[controller]")]
public sealed class RolesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RoleResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> GetRoles(
        CancellationToken cancellationToken)
    {
        var roles = await sender.Send(new GetRolesQuery(), cancellationToken);
        return Ok(roles.Select(ToResponse));
    }

    [HttpGet("{roleId:guid}")]
    public async Task<ActionResult<RoleResponse>> GetById(Guid roleId, CancellationToken cancellationToken)
    {
        var role = await sender.Send(new GetRoleQuery(roleId), cancellationToken);
        return Ok(ToResponse(role));
    }

    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create([FromBody] SaveRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await sender.Send(new SaveRoleCommand(null, request.Name), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { roleId = role.Id }, ToResponse(role));
    }

    [HttpPut("{roleId:guid}")]
    public async Task<ActionResult<RoleResponse>> Update(Guid roleId, [FromBody] SaveRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await sender.Send(new SaveRoleCommand(roleId, request.Name), cancellationToken);
        return Ok(ToResponse(role));
    }

    [HttpDelete("{roleId:guid}")]
    public async Task<IActionResult> Delete(Guid roleId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteRoleCommand(roleId), cancellationToken);
        return NoContent();
    }

    [HttpGet("permissions")]
    [ProducesResponseType<IReadOnlyList<PermissionResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PermissionResponse>>> GetPermissions(
        CancellationToken cancellationToken)
    {
        var permissions = await sender.Send(new GetPermissionsQuery(), cancellationToken);
        return Ok(permissions.Select(permission => new PermissionResponse(
            permission.Code,
            permission.Name,
            permission.Description)));
    }

    [HttpPut("{roleId:guid}/permissions")]
    [ProducesResponseType<RoleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> UpdatePermissions(
        Guid roleId,
        [FromBody] UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await sender.Send(
                new UpdateRolePermissionsCommand(roleId, request.PermissionCodes),
                cancellationToken);

            return Ok(ToResponse(role));
        }
        catch (ResourceNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (CommandValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private static RoleResponse ToResponse(RolePermissionsResult role)
    {
        return new RoleResponse(
            role.Id,
            role.Name,
            role.HasAllPermissions,
            role.PermissionCodes);
    }
}
