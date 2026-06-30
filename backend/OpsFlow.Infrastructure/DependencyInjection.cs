using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpsFlow.Application.Interfaces;
using OpsFlow.Infrastructure.Authentication;
using OpsFlow.Infrastructure.Persistence;

namespace OpsFlow.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    services.AddDbContext<AppDbContext>(options =>
    {
      options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection"));
    });

    services.Configure<JwtSettings>(
      configuration.GetSection("Jwt"));

    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<OpsFlow.Application.Interfaces.IAuthService, OpsFlow.Application.Services.AuthService>();
    services.AddScoped<OpsFlow.Application.Interfaces.IUserRepository, OpsFlow.Infrastructure.Repositories.UserRepository>();
    services.AddScoped<OpsFlow.Application.Interfaces.IPasswordHasher, OpsFlow.Infrastructure.Authentication.PasswordHasher>();

    return services;
  }
}