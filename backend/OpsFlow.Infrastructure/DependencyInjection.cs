using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpsFlow.Application.Interfaces;
using OpsFlow.Infrastructure.Authentication;
using OpsFlow.Infrastructure.Persistence;
using MediatR;
using OpsFlow.Application.Services;
using OpsFlow.Application.Mappings;

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
    services.AddScoped<OpsFlow.Application.Interfaces.IRequestRepository, OpsFlow.Infrastructure.Repositories.RequestRepository>();
    services.AddScoped<OpsFlow.Application.Interfaces.IDashboardRepository, OpsFlow.Infrastructure.Repositories.DashboardRepository>();
    services.AddScoped<OpsFlow.Application.Interfaces.IPasswordHasher, OpsFlow.Infrastructure.Authentication.PasswordHasher>();
    services.AddScoped<OpsFlow.Application.Interfaces.IAuditService, OpsFlow.Infrastructure.Services.AuditService>();
    services.AddScoped<OpsFlow.Application.Services.RequestService>();
    services.AddScoped<OpsFlow.Application.Services.DashboardService>();
    services.AddSingleton<IResponseMapper, ResponseMapper>();

    // MediatR registration for application handlers
    services.AddMediatR(typeof(RequestService).Assembly);

    return services;
  }
}