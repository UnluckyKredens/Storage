using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIApplication.Modules.Users;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Authentication;

public sealed class UpdateOwnAccountCommandHandler(
    IRepository<User> userRepository,
    IRepository<Role> roleRepository,
    IRepository<Warehouse> warehouseRepository,
    IPasswordHashingService passwordHashingService,
    IJwtTokenService jwtTokenService) : ICommandHandler<UpdateOwnAccountCommand, UpdatedAccountResult>
{
    public async ValueTask<UpdatedAccountResult> Handle(
        UpdateOwnAccountCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(
            candidate => candidate.Id == command.UserId, cancellationToken)
            ?? throw new ResourceNotFoundException("Użytkownik nie został znaleziony.");

        var login = command.Login.Trim();
        var firstName = command.FirstName.Trim();
        var lastName = command.LastName.Trim();
        var email = command.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(firstName)
            || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
            throw new CommandValidationException("Login, imię, nazwisko i e-mail są wymagane.");

        if (!string.IsNullOrEmpty(command.NewPassword))
        {
            if (command.NewPassword.Length < 8)
                throw new CommandValidationException("Nowe hasło musi mieć co najmniej 8 znaków.");
            if (string.IsNullOrEmpty(command.CurrentPassword)
                || !passwordHashingService.VerifyPassword(command.CurrentPassword, user.PasswordHash))
                throw new CommandValidationException("Nieprawidłowe obecne hasło.");
        }

        var loginOwner = await userRepository.FirstOrDefaultAsync(
            candidate => candidate.Login == login, cancellationToken);
        if (loginOwner is not null && loginOwner.Id != user.Id)
            throw new ResourceConflictException("Inny użytkownik korzysta już z tego loginu.");

        var emailOwner = await userRepository.FirstOrDefaultAsync(
            candidate => candidate.Email == email, cancellationToken);
        if (emailOwner is not null && emailOwner.Id != user.Id)
            throw new ResourceConflictException("Inny użytkownik korzysta już z tego adresu e-mail.");

        var role = await roleRepository.FirstOrDefaultAsync(
            candidate => candidate.Id == user.RoleId, cancellationToken)
            ?? throw new CommandValidationException("Rola użytkownika nie istnieje.");
        var warehouse = user.WarehouseId is null ? null : await warehouseRepository.FirstOrDefaultAsync(
            candidate => candidate.WarehouseId == user.WarehouseId, cancellationToken);

        user.Login = login;
        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;
        if (!string.IsNullOrEmpty(command.NewPassword))
            user.PasswordHash = passwordHashingService.HashPassword(command.NewPassword);

        await userRepository.UpdateAsync(user, cancellationToken);
        var token = jwtTokenService.CreateToken(user, role.Name).AccessToken;
        var view = new UserView(user.Id, user.Login, user.FirstName, user.LastName,
            user.Email, user.RoleId, role.Name, user.WarehouseId, warehouse?.Name);
        return new UpdatedAccountResult(token, view);
    }
}
