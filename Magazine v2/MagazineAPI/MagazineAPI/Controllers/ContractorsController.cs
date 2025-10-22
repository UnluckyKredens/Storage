using MagazineAPI.Authorization;
using MagazineAPIApplication.Modules.Contractors;
using MagazineAPIDomain.Authorization;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ContractorsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionCodes.ContractorsRead)]
    public async Task<ActionResult<IReadOnlyList<ContractorView>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetContractorsQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionCodes.ContractorsRead)]
    public async Task<ActionResult<ContractorView>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetContractorQuery(id), cancellationToken));
    }

    [HttpPost]
    [HasPermission(PermissionCodes.ContractorsManage)]
    public async Task<ActionResult<ContractorView>> Create([FromBody] SaveContractorCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = null }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionCodes.ContractorsManage)]
    public async Task<ActionResult<ContractorView>> Update(Guid id, [FromBody] SaveContractorCommand command, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(command with { Id = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionCodes.ContractorsManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteContractorCommand(id), cancellationToken);
        return NoContent();
    }
}
