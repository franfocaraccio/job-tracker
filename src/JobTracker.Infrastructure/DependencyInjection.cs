using System.Text;
using JobTracker.Application.JobApplications;
using JobTracker.Application.Users;
using JobTracker.Infrastructure.Authentication;
using JobTracker.Infrastructure.JobApplications;
using JobTracker.Infrastructure.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace JobTracker.Infrastructure;

/// <summary>
/// Registers PostgreSQL access, password hashing and JWT authentication.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()!;

        services.AddSingleton(NpgsqlDataSource.Create(configuration.GetConnectionString("Postgres")!));
        services.AddSingleton(jwtSettings);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
                };
            });

        return services;
    }
}
