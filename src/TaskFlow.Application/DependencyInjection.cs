using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaskFlow.Application.Common.Behaviors;

namespace TaskFlow.Application;

/// <summary>
/// Extension methods for registering Application layer services in the DI container.
/// This keeps Program.cs clean and organizes service registration by layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Application layer services to the DI container.
    /// Call this from Program.cs in the API project.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // Register FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Register AutoMapper
        services.AddAutoMapper(assembly);

        // Register MediatR Pipeline Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        return services;
    }
}