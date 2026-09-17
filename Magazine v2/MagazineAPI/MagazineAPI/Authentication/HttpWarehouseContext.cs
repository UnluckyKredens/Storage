using System.Security.Claims;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Authorization;

namespace MagazineAPI.Authentication;

public sealed class HttpWarehouseContext(IHttpContextAccessor httpContextAccessor)
    : IWarehouseContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId => ReadClaim(ClaimTypes.NameIdentifier);

    public bool IsAdministrator => ReadClaim("role_id") == RoleIds.Administrator;

    public Guid? WarehouseId
    {
        get
        {
            var assignedWarehouseId = ReadClaim("warehouse_id");
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
            return Guid.TryParse(header, out var warehouseId) ? warehouseId : null;
        }
    }

    public bool CanAccess(Guid warehouseId)
    {
        return IsAdministrator
            ? WarehouseId is null || WarehouseId == warehouseId
            : WarehouseId == warehouseId;
    }

    private Guid? ReadClaim(string claimType)
    {
        var value = User?.FindFirstValue(claimType);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
