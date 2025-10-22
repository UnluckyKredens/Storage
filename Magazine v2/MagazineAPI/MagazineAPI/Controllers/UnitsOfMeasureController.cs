using MagazineAPI.Authorization;
using MagazineAPIApplication.Modules.UnitsOfMeasure;
using MagazineAPIDomain.Authorization;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[HasPermission(PermissionCodes.DictionariesManage)]
[Route("api/[controller]")]
public sealed class UnitsOfMeasureController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UnitOfMeasureView>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetUnitsOfMeasureQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UnitOfMeasureView>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetUnitOfMeasureQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<UnitOfMeasureView>> Create([FromBody] SaveUnitOfMeasureCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = null }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UnitOfMeasureView>> Update(Guid id, [FromBody] SaveUnitOfMeasureCommand command, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(command with { Id = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteUnitOfMeasureCommand(id), cancellationToken);
        return NoContent();
    }
}
