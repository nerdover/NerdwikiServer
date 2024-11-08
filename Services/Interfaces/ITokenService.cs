using Microsoft.AspNetCore.Identity;

namespace NerdwikiServer.Services.Interfaces;

public interface ITokenService
{
    public string GenerateAccessToken(IdentityUser user);
    public string GenerateRefreshToken();
}