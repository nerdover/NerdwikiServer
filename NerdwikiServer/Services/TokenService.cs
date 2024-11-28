using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NerdwikiServer.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NerdwikiServer.Services;

public class TokenService(IConfiguration configuration, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager) : ITokenService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task<string> GenerateAccessToken(IdentityUser user)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];

        var roles = await _userManager.GetRolesAsync(user);
        //var allRoleClaims = new HashSet<Claim>();

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));

            //var foundRole = await _roleManager.FindByNameAsync(role);
            //if (foundRole is null)
            //{
            //    continue;
            //}
            //var roleClaims = await _roleManager.GetClaimsAsync(foundRole);
            //foreach (var claim in roleClaims)
            //{
            //    allRoleClaims.Add(claim);
            //}
        }

        //claims.AddRange(allRoleClaims);

        var rawKey = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
        var key = new SymmetricSecurityKey(rawKey);

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(10),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }
}
