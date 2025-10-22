using System.Security.Claims;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Authorization;

namespace MagazineAPI.Authentication;

public sealed class HttpWarehouseContext(IHttpContextAccessor httpContextAccessor)
    : IWarehouseContext
{
    private AuthenticatedAccount? Account => httpContextAccessor.HttpContext?.Items[AuthenticatedAccount.ContextKey] as AuthenticatedAccount;

    public Guid? UserId => Account?.UserId;

    public bool IsAdministrator => Account?.RoleId == RoleIds.Administrator;

    public Guid? WarehouseId
    {
        get
        {
            var assignedWarehouseId = Account?.WarehouseId;
            if (assignedWarehouseId is not null)
            {
                return assignedWarehouseId;
            }

            if (!IsAdministrator)
            {
                return null;
            }

            var header = httpContextAccessor.HttpContext?.Request.Headers["X-Warehouse-Id"]
                .FirstOrDefault();
            if (string.IsNullOrWhiteSpace(header)) return null;
            return Guid.TryParse(header, out var warehouseId) && warehouseId != Guid.Empty
                ? warehouseId
                : throw new MagazineAPIApplication.Common.Exceptions.CommandValidationException("Niepoprawny nagłówek magazynu.");
        }
    }

    public bool CanAccess(Guid warehouseId)
    {
        return IsAdministrator
            ? WarehouseId is null || WarehouseId == warehouseId
            : WarehouseId == warehouseId;
    }

}
