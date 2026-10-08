using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIApplication.Modules.Authentication;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Authentication;

public sealed class LoginUserCommandHandler(
    IRepository<User> userRepository,
    IRepository<Role> roleRepository,
    IPasswordHashingService passwordHashingService,
    IJwtTokenService jwtTokenService) : ICommandHandler<LoginUserCommand, AuthenticationResult>
{
    public async ValueTask<AuthenticationResult> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var loginOrEmail = command.Login.Trim();
        var normalizedEmail = NormalizeEmail(loginOrEmail);

        var user = await userRepository.FirstOrDefaultAsync(
            candidate => candidate.Login == loginOrEmail || candidate.Email == normalizedEmail,
            cancellationToken);
        if (user is null || !passwordHashingService.VerifyPassword(command.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Nieprawidłowy login, e-mail lub hasło.");
        }

        var role = await roleRepository.FirstOrDefaultAsync(candidate => candidate.Id == user.RoleId, cancellationToken);
        if (role is null)
        {
            throw new ResourceNotFoundException("Brak przypisanej roli użytkownika.");
        }

        var token = jwtTokenService.CreateToken(user, role.Name);
        return new AuthenticationResult(token.AccessToken);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
