using System.Security.Claims;
using MagazineAPI.Authorization;
using MagazineAPI.Contracts.Users;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Modules.Users;
using MagazineAPIApplication.Modules.Warehouses;
using MagazineAPIApplication.Modules.Roles;
using MagazineAPIDomain.Authorization;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionCodes.UsersRead)]
    public async Task<ActionResult<IReadOnlyList<UserView>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetUsersQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionCodes.UsersRead)]
    public async Task<ActionResult<UserView>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetUserQuery(id), cancellationToken));
    }

    [HttpGet("page-data")]
    [HasPermission(PermissionCodes.UsersManage)]
    public async Task<ActionResult> PageData(CancellationToken cancellationToken)
    {
        var roles = await sender.Send(new GetRolesQuery(), cancellationToken);
        var warehouses = await sender.Send(new GetWarehousesQuery(), cancellationToken);
        return Ok(new { roles, warehouses });
    }

    [HttpPost]
    [HasPermission(PermissionCodes.UsersManage)]
    public async Task<ActionResult<UserView>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await sender.Send(new CreateUserCommand(request.Login, request.FirstName,
            request.LastName, request.Email, request.Password, request.RoleId, request.WarehouseId,
            GetCurrentUserId()), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionCodes.UsersManage)]
    [ProducesResponseType<UserView>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserView>> Update(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(
                new UpdateUserCommand(
                    id,
                    request.Login,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Password,
                    request.RoleId,
                    request.WarehouseId,
                    GetCurrentUserId()),
                cancellationToken);

            return Ok(result);
        }
        catch (ResourceNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (ResourceConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (CommandValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (ForbiddenOperationException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionCodes.UsersManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(
                new DeleteUserCommand(id, GetCurrentUserId()),
                cancellationToken);
            return NoContent();
        }
        catch (ResourceNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (CommandValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (ForbiddenOperationException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = exception.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
