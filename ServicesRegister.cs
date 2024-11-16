using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NerdwikiServer.Data;
using NerdwikiServer.Repositories;
using NerdwikiServer.Repositories.Interfaces;
using NerdwikiServer.Services;
using NerdwikiServer.Services.Interfaces;

namespace NerdwikiServer;

public static class ServicesRegister
{
    public static void RegisterCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString)
        );

        // Configure Identity
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
            options.User.RequireUniqueEmail = true
        )
        .AddEntityFrameworkStores<ApplicationDbContext>();

        // Configure JWT authentication
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
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

        // Register repositories
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<ISeriesRepository, SeriesRepository>();
        services.AddScoped<ISeriesLessonRepository, SeriesLessonRepository>();

        // Register services
        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();
    }
}