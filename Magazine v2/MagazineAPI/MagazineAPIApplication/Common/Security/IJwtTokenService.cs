using MagazineAPIDomain.Entities;

namespace MagazineAPIApplication.Common.Security;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(User user, string roleName);
}
