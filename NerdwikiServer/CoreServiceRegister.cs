using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NerdwikiServer.Data;
using NerdwikiServer.Services;
using NerdwikiServer.Services.Interfaces;

namespace NerdwikiServer;

public static class CoreServiceRegister
{
    public static void RegisterCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString)
        );

        services.AddIdentity<IdentityUser, IdentityRole>(options =>
            options.User.RequireUniqueEmail = true
        ).AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddAuthorizationBuilder()
            //.AddPolicy("CanViewContent", policy =>
            //    policy.RequireClaim("Permission", "CanViewContent")
            //)
            .AddPolicy("AdminAccess", policy =>
                policy.RequireRole("Admin")
            );

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = key
            };
        });

        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();
    }
}
