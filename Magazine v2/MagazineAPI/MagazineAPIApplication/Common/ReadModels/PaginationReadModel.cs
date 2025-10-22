namespace MagazineAPIApplication.Common.ReadModels;

public class PaginationReadModel<T>
{
    public int Total { get; set; }
    public List<T> List { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
}
