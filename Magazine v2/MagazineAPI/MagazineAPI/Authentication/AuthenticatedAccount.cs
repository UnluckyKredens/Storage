namespace MagazineAPI.Authentication;

public sealed record AuthenticatedAccount(Guid UserId, Guid RoleId, Guid? WarehouseId)
{
    public static readonly object ContextKey = new();
}
