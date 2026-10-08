using MagazineAPI.Authorization;
using MagazineAPIApplication.Modules.Categories;
using MagazineAPIDomain.Authorization;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[HasPermission(PermissionCodes.DictionariesManage)]
[Route("api/[controller]")]
public sealed class CategoryController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryView>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetCategoriesQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryView>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetCategoryQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryView>> Create([FromBody] SaveCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = null }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryView>> Update(Guid id, [FromBody] SaveCategoryCommand command, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(command with { Id = id }, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}
