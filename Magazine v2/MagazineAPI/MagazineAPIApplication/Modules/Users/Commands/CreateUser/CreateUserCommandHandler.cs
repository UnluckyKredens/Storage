using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Users;

public sealed class CreateUserCommandHandler(
    IRepository<User> userRepository,
    IRepository<Role> roleRepository,
    IRepository<Warehouse> warehouseRepository,
    IRolePermissionRepository rolePermissionRepository,
    IPasswordHashingService passwordHashingService,
    IWarehouseContext warehouseContext) : ICommandHandler<CreateUserCommand, UserView>
{
    public async ValueTask<UserView> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Login) || string.IsNullOrWhiteSpace(command.FirstName)
            || string.IsNullOrWhiteSpace(command.LastName) || string.IsNullOrWhiteSpace(command.Email)
            || string.IsNullOrWhiteSpace(command.Password))
            throw new CommandValidationException("Dane użytkownika i hasło są wymagane.");
        if (command.Password.Length < 8)
            throw new CommandValidationException("Hasło musi mieć co najmniej 8 znaków.");

        var role = await roleRepository.FirstOrDefaultAsync(x => x.Id == command.RoleId, cancellationToken)
            ?? throw new CommandValidationException("Rola nie istnieje.");
        if (!await rolePermissionRepository.UserIsAdministratorAsync(command.ActorUserId, cancellationToken)
            && role.Id != RoleIds.User)
            throw new ForbiddenOperationException("Tylko administrator może przypisać inną rolę niż Pracownik.");
        if (!warehouseContext.IsAdministrator
            && command.WarehouseId != warehouseContext.WarehouseId)
            throw new ForbiddenOperationException("Nie można przypisać użytkownika do innego magazynu.");

        Warehouse? warehouse = null;
        if (role.Id != RoleIds.Administrator)
        {
            if (command.WarehouseId is null)
                throw new CommandValidationException("Magazyn jest wymagany dla użytkownika, który nie jest administratorem.");
            warehouse = await warehouseRepository.FirstOrDefaultAsync(
                x => x.WarehouseId == command.WarehouseId, cancellationToken)
                ?? throw new CommandValidationException("Wybrany magazyn nie istnieje.");
        }

        var login = command.Login.Trim();
        var email = command.Email.Trim().ToLowerInvariant();
        if (await userRepository.FirstOrDefaultAsync(x => x.Login == login, cancellationToken) is not null)
            throw new ResourceConflictException("Login jest już zajęty.");
        if (await userRepository.FirstOrDefaultAsync(x => x.Email == email, cancellationToken) is not null)
            throw new ResourceConflictException("Adres e-mail jest już zajęty.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = login,
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            Email = email,
            PasswordHash = passwordHashingService.HashPassword(command.Password),
            RoleId = role.Id,
            WarehouseId = warehouse?.WarehouseId
        };
        await userRepository.AddAsync(user, cancellationToken);
        return new UserView(user.Id, user.Login, user.FirstName, user.LastName,
            user.Email, user.RoleId, role.Name, user.WarehouseId, warehouse?.Name);
    }
}
