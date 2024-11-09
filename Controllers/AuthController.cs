using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NerdwikiServer.Data;
using NerdwikiServer.Data.Base;
using NerdwikiServer.Data.Entities;
using NerdwikiServer.Dtos;
using NerdwikiServer.Repositories.Interfaces;
using NerdwikiServer.Services.Interfaces;

namespace NerdwikiServer.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IConfiguration configuration,
    ApplicationDbContext context,
    ITokenService tokenService
) : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly SignInManager<IdentityUser> _signInManager = signInManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly ApplicationDbContext _context = context;
    private readonly ITokenService _tokenService = tokenService;

    public record SignUpRequest(string Username, string Email, string Password);
    public record SignInRequest(string Username, string Password);

    [AllowAnonymous]
    [HttpPost("signUp")]
    public async Task<IActionResult> AuthSignUp(SignUpRequest request)
    {
        var user = new IdentityUser { UserName = request.Username, Email = request.Email };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            return CreatedAtAction(nameof(AuthSignUp),
                new ServerResponse { Success = true, Message = "Successfully Signed Up." }
            );
        }

        return BadRequest(
            new ServerResponse<IEnumerable<string>>
            {
                Success = false,
                Message = "Invalid Request.",
                Data = result.Errors.Select(e => e.Description)
            }
        );
    }

    [AllowAnonymous]
    [HttpPost("signIn")]
    public async Task<IActionResult> AuthSignIn(SignInRequest request)
    {
        // Step : Find User
        var user = await _userManager.FindByNameAsync(request.Username);

        if (user is null)
        {
            return Unauthorized();
        }

        // Step : Check Password
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        // Step : Generate Access Token and Refresh Token
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Step : Store Refresh Token in Database
        await _userManager.SetAuthenticationTokenAsync(
            user,
            _configuration["Jwt:Issuer"]!,
            "refresh_token",
            refreshToken
        );

        // Step : Set Refresh Token Cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.Now.AddDays(7)
        };
        HttpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

        // Step : Return Access Token
        return Ok(new ServerResponse<string> { Success = true, Message = "Successfully Signed In .", Data = accessToken });
    }

    [HttpPost("signOut")]
    public async Task<IActionResult> AuthSignOut()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is not null)
        {
            await _userManager.RemoveAuthenticationTokenAsync(user, _configuration["Jwt:Issuer"]!, "refresh_token");
        }

        return NoContent();
    }

    [HttpGet]
    public IActionResult GetAuthenticationStatus()
    {
        var status = User.Identity?.IsAuthenticated ?? false;
        return Ok(new ServerResponse { Success = status, Message = status ? "Authenticated" : "Not Authenticated" });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        // Step : Get Refresh Token from Cookie
        var refreshToken = HttpContext.Request.Cookies["refreshToken"];

        // Step : Get Refresh Token from Server Storage
        var userToken = await _context.UserTokens.FirstOrDefaultAsync(ut => ut.Value == refreshToken);

        if (userToken is null || userToken.LoginProvider != _configuration["Jwt:Issuer"] || userToken.Name != "refresh_token")
        {
            return Unauthorized();
        }

        // Step : Find User From Token
        var user = await _userManager.FindByIdAsync(userToken.UserId);

        if (user is null)
        {
            return Unauthorized();
        }

        // Step : Generate Access Token and Refresh Token
        var accessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Step : Store Refresh Token in Database
        await _userManager.SetAuthenticationTokenAsync(
            user,
            _configuration["Jwt:Issuer"]!,
            "refresh_token",
            newRefreshToken
        );

        // Step : Set Refresh Token Cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.Now.AddDays(7)
        };
        HttpContext.Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

        // Step : Return Access Token
        return Ok(new ServerResponse<string> { Success = true, Message = "Refresh token successfully.", Data = accessToken });
    }
}