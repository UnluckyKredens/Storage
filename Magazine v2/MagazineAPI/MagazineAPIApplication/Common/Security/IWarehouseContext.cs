namespace MagazineAPIApplication.Common.Security;

public interface IWarehouseContext
{
    Guid? UserId { get; }
    bool IsAdministrator { get; }
    Guid? WarehouseId { get; }
    bool CanAccess(Guid warehouseId);
}
