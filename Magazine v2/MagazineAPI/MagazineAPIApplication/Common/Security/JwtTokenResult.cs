namespace MagazineAPIApplication.Common.Security;

public sealed record JwtTokenResult(string AccessToken, DateTime ExpiresAtUtc);
