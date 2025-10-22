using MagazineAPIApplication.Common.ReadModels;
using Mediator;

namespace MagazineAPIApplication.Modules.Products;

public sealed record GetProductQuery(Guid Id) : IQuery<ProductReadModel>;
