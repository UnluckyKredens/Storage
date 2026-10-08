using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Users;

public sealed class UpdateUserCommandHandler(
    IRepository<User> userRepository,
    IRepository<Role> roleRepository,
    IRepository<Warehouse> warehouseRepository,
    IRolePermissionRepository rolePermissionRepository,
    IPasswordHashingService passwordHashingService,
    IWarehouseContext warehouseContext) : ICommandHandler<UpdateUserCommand, UserView>
{
    public async ValueTask<UserView> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(candidate => candidate.Id == command.UserId, cancellationToken);
        if (user is null)
        {
            throw new ResourceNotFoundException("Użytkownik nie został znaleziony.");
        }

        var actorIsAdministrator = await rolePermissionRepository.UserIsAdministratorAsync(
            command.ActorUserId,
            cancellationToken);

        if (!actorIsAdministrator
            && (user.RoleId == RoleIds.Administrator || command.RoleId != user.RoleId))
        {
            throw new ForbiddenOperationException(
                "Tylko administrator może modyfikować administratorów lub zmieniać role użytkowników.");
        }

        if (!actorIsAdministrator
            && (user.WarehouseId != warehouseContext.WarehouseId
                || command.WarehouseId != warehouseContext.WarehouseId))
        {
            throw new ForbiddenOperationException(
                "Nie można modyfikować użytkownika z innego magazynu.");
        }

        if (user.RoleId == RoleIds.Administrator
            && command.RoleId != RoleIds.Administrator
            && await rolePermissionRepository.CountUsersInRoleAsync(
                RoleIds.Administrator,
                cancellationToken) == 1)
        {
            throw new CommandValidationException(
                "Nie można odebrać roli ostatniemu administratorowi.");
        }

        var normalizedLogin = NormalizeLogin(command.Login);
        var loginOwner = await userRepository.FirstOrDefaultAsync(candidate => candidate.Login == normalizedLogin, cancellationToken);
        if (loginOwner is not null && loginOwner.Id != command.UserId)
        {
            throw new ResourceConflictException("Inny użytkownik korzysta już z tego loginu.");
        }

        var normalizedEmail = NormalizeEmail(command.Email);
        var emailOwner = await userRepository.FirstOrDefaultAsync(candidate => candidate.Email == normalizedEmail, cancellationToken);
        if (emailOwner is not null && emailOwner.Id != command.UserId)
        {
            throw new ResourceConflictException("Inny użytkownik korzysta już z tego adresu e-mail.");
        }

        var role = await roleRepository.FirstOrDefaultAsync(candidate => candidate.Id == command.RoleId, cancellationToken);
        if (role is null)
        {
            throw new CommandValidationException("Podana rola nie istnieje.");
        }

        Warehouse? warehouse = null;
        if (role.Id != RoleIds.Administrator)
        {
            if (command.WarehouseId is null)
                throw new CommandValidationException("Magazyn jest wymagany dla użytkownika, który nie jest administratorem.");
            warehouse = await warehouseRepository.FirstOrDefaultAsync(
                candidate => candidate.WarehouseId == command.WarehouseId, cancellationToken)
                ?? throw new CommandValidationException("Wybrany magazyn nie istnieje.");
        }

        user.Login = normalizedLogin;
        user.FirstName = command.FirstName.Trim();
        user.LastName = command.LastName.Trim();
        user.Email = normalizedEmail;
        user.RoleId = role.Id;
        user.WarehouseId = warehouse?.WarehouseId;

        if (!string.IsNullOrWhiteSpace(command.Password))
        {
            user.PasswordHash = passwordHashingService.HashPassword(command.Password);
        }

        await userRepository.UpdateAsync(user, cancellationToken);

        return new UserView(user.Id, user.Login, user.FirstName, user.LastName, user.Email,
            role.Id, role.Name, user.WarehouseId, warehouse?.Name);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string NormalizeLogin(string login)
    {
        return login.Trim();
    }
}
