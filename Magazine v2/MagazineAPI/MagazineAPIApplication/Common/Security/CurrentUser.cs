using System.Security.Claims;
using MagazineAPIApplication.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MagazineAPIApplication.Common.Security;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            if (!Guid.TryParse(value, out var id))
                throw new UnauthorizedAccessException();

            return id;
        }
    }
}