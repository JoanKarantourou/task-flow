using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Identity;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services in the DI container.
/// This keeps Program.cs clean and organizes service registration by layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Infrastructure layer services to the DI container.
    /// Call this from Program.cs in the API project.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // ========================================
        // Register Unit of Work & Repositories
        // ========================================
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();

        // ========================================
        // Register Authentication Services
        // ========================================
        // TokenService generates and validates JWT tokens
        services.AddScoped<ITokenService, TokenService>();

        // AuthService handles registration, login, token refresh
        services.AddScoped<IAuthService, AuthService>();

        // CurrentUserService extracts user info from HTTP context
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}