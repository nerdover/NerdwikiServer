using Microsoft.AspNetCore.Identity;

namespace NerdwikiServer.Services.Interfaces;

public interface ITokenService
{
    public Task<string> GenerateAccessToken(IdentityUser user);
    public string GenerateRefreshToken();
}
