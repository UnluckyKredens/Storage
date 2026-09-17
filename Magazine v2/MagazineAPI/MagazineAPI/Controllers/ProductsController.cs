using MagazineAPI.Authorization;
using MagazineAPIApplication.Common.ReadModels;
using MagazineAPIApplication.Modules.Products;
using MagazineAPIApplication.Modules.Categories;
using MagazineAPIApplication.Modules.UnitsOfMeasure;
using MagazineAPIDomain.Authorization;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionCodes.ProductsRead)]
    public async Task<ActionResult<PaginationReadModel<ProductReadModel>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string search = "", [FromQuery] string sortBy = "name",
        [FromQuery] string order = "asc",
        CancellationToken cancellationToken = default)
    {
        return Ok(await sender.Send(new GetProductsQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            OrderBy = order
        }, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionCodes.ProductsRead)]
    public async Task<ActionResult<ProductReadModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetProductQuery(id), cancellationToken));
    }

    [HttpGet("page-data")]
    [HasPermission(PermissionCodes.ProductsRead)]
    public async Task<ActionResult> PageData(CancellationToken cancellationToken)
    {
        var categories = await sender.Send(new GetCategoriesQuery(), cancellationToken);
        var units = await sender.Send(new GetUnitsOfMeasureQuery(), cancellationToken);
        return Ok(new { categories, units });
    }

    [HttpPost]
    [HasPermission(PermissionCodes.ProductsManage)]
    public async Task<ActionResult> Create([FromBody] SaveProductCommand command, CancellationToken cancellationToken)
    {
        command.ProductId = null;
        var id = await sender.Send(command, cancellationToken);
        var product = await sender.Send(new GetProductQuery(id), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, product);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionCodes.ProductsManage)]
    public async Task<ActionResult<ProductReadModel>> Update(Guid id, [FromBody] SaveProductCommand command, CancellationToken cancellationToken)
    {
        command.ProductId = id;
        await sender.Send(command, cancellationToken);
        return Ok(await sender.Send(new GetProductQuery(id), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionCodes.ProductsManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }
}
