namespace MagazineAPI.Authentication;

// Populated from the database by JWT validation, never from client headers.
public sealed record AuthenticatedAccount(Guid UserId, Guid RoleId, Guid? WarehouseId)
{
    public static readonly object ContextKey = new();
}
