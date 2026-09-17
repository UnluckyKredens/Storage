using System.Security.Claims;
using MagazineAPI.Contracts.Authentication;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Modules.Authentication;
using MagazineAPIApplication.Modules.Users;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ISender sender) : ControllerBase
{

    [Authorize]
    [HttpGet("IsAuthenticated")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<bool> IsAuthenticated()
    {
        return true;
    }
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(
                new LoginUserCommand(request.Login, request.Password),
                cancellationToken);

            return Ok(new AuthResponse(result.Token));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
        catch (ResourceNotFoundException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: exception.Message);
        }
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<UserView>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserView>> Me(CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idClaim, out var userId))
            return Unauthorized(new { message = "Token nie zawiera identyfikatora użytkownika." });

        return Ok(await sender.Send(new GetUserQuery(userId), cancellationToken));
    }

    [Authorize]
    [HttpPut("me")]
    [ProducesResponseType<UpdatedAccountResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdatedAccountResult>> UpdateMe(
        [FromBody] UpdateOwnAccountRequest request,
        CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idClaim, out var userId))
            return Unauthorized(new { message = "Token nie zawiera identyfikatora użytkownika." });

        var result = await sender.Send(new UpdateOwnAccountCommand(
            userId, request.Login, request.FirstName, request.LastName,
            request.Email, request.CurrentPassword, request.NewPassword), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me/permissions")]
    [ProducesResponseType<PermissionsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PermissionsResponse>> MyPermissions(
        CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idClaim, out var userId))
        {
            return Unauthorized(new { message = "Token nie zawiera identyfikatora użytkownika." });
        }

        var permissions = await sender.Send(
            new GetUserPermissionsQuery(userId),
            cancellationToken);

        return Ok(new PermissionsResponse(permissions));
    }
}
