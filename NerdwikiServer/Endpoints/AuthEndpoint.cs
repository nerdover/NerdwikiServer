using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NerdwikiServer.Data;
using NerdwikiServer.Services.Interfaces;

namespace NerdwikiServer.Endpoints;

public static class AuthEndpoint
{
    private record SignUpRequest(string Username, string Email, string Password);
    private record SignInRequest(string Username, string Password);
    private record CreateRoleDto(string RoleName);
    private record CreateRoleClaimDto(string RoleName, string ClaimType, string ClaimValue);
    private record AssignRoleDto(string Username, string RoleName);

    public static WebApplication MapAuthApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapGet("/", GetStatus);
        group.MapPost("/signup", SignUp);
        group.MapPost("/signin", SignIn);
        group.MapPost("/signout", SignOut).RequireAuthorization();
        group.MapPost("/refresh", RefreshToken).RequireAuthorization();
        group.MapPost("/role", AddRole).RequireAuthorization("AdminAccess");
        group.MapPost("/roleclaim", AddRoleClaim).RequireAuthorization("AdminAccess");
        group.MapPost("/assign-role", AssignRole);
            //.RequireAuthorization("AdminAccess");

        return app;
    }

    private static async Task<IResult> SignUp(SignUpRequest request, UserManager<IdentityUser> userManager)
    {
        var user = new IdentityUser { UserName = request.Username, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return TypedResults.BadRequest(result.Errors);
        }

        return TypedResults.Ok("User registered successfully.");
    }

    private static async Task<IResult> SignIn(
        SignInRequest request, 
        UserManager<IdentityUser> userManager, 
        SignInManager<IdentityUser> signInManager, 
        ITokenService tokenService, 
        IConfiguration configuration,
        HttpContext httpContext
    )
    {
        var user = await userManager.FindByNameAsync(request.Username);

        if (user is null)
            return TypedResults.Unauthorized();

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!result.Succeeded)
            return TypedResults.Unauthorized();

        var accessToken = await tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        await userManager.SetAuthenticationTokenAsync(
            user,
            configuration["Jwt:Issuer"]!,
            "refresh_token",
            refreshToken
        );

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.Now.AddDays(7)
        };
        httpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);


        return TypedResults.Ok(accessToken);
    }

    private static async Task<IResult> SignOut(
        UserManager<IdentityUser> userManager,
        IConfiguration configuration,
        HttpContext httpContext
    )
    {
        var user = await userManager.GetUserAsync(httpContext.User);

        if (user is not null)
            await userManager.RemoveAuthenticationTokenAsync(user, configuration["Jwt:Issuer"]!, "refresh_token");

        return TypedResults.NoContent();
    }

    private static Ok<bool> GetStatus(HttpContext httpContext)
    {
        return TypedResults.Ok(httpContext.User.Identity?.IsAuthenticated ?? false);
    }

    private static async Task<IResult> RefreshToken(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        ITokenService tokenService,
        IConfiguration configuration,
        HttpContext httpContext
    )
    {
        var refreshToken = httpContext.Request.Cookies["refreshToken"];

        if (refreshToken is null)
            return TypedResults.Unauthorized();

        var userToken = await context.UserTokens.FirstOrDefaultAsync(ut => ut.Value == refreshToken);

        if (userToken is null || userToken.LoginProvider != configuration["Jwt:Issuer"] || userToken.Name != "refresh_token")
            return TypedResults.Unauthorized();

        var user = await userManager.FindByIdAsync(userToken.UserId);

        if (user is null)
            return TypedResults.Unauthorized();

        var accessToken = await tokenService.GenerateAccessToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        await userManager.SetAuthenticationTokenAsync(
            user,
            configuration["Jwt:Issuer"]!,
            "refresh_token",
            refreshToken
        );

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.Now.AddDays(7)
        };
        httpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

        return TypedResults.Ok(accessToken);
    }

    private static async Task<IResult> AssignRole(AssignRoleDto dto, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        var user = await userManager.FindByNameAsync(dto.Username);
        if (user is null)
            return TypedResults.NotFound($"User with name '{dto.Username}' is not found.");

        var roleExist = await roleManager.RoleExistsAsync(dto.RoleName);
        if (!roleExist)
            return TypedResults.NotFound($"Role with name '{dto.RoleName}' is not found.");

        var result = await userManager.AddToRoleAsync(user, dto.RoleName);
        if (!result.Succeeded)
            return TypedResults.Problem();

        return TypedResults.NoContent();
    }

    private static async Task<IResult> AddRole(CreateRoleDto dto, RoleManager<IdentityRole> roleManager)
    {
        IdentityRole role = new(dto.RoleName);
        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return TypedResults.Problem();

        return TypedResults.Ok(role);
    }

    private static async Task<IResult> AddRoleClaim(CreateRoleClaimDto dto, RoleManager<IdentityRole> roleManager)
    {
        var role = await roleManager.FindByNameAsync(dto.RoleName);
        if (role is null)
            return TypedResults.NotFound();

        var claim = new Claim(dto.ClaimType, dto.ClaimValue);
        var result = await roleManager.AddClaimAsync(role, claim);
        if (!result.Succeeded)
            return TypedResults.Problem();

        return TypedResults.Ok(new { role, claim });
    }
}
