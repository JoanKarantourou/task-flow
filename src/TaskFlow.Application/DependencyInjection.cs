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
/// <remarks>
/// This class registers:
/// - MediatR with all command/query handlers
/// - FluentValidation with all validators
/// - AutoMapper with all mapping profiles
/// - Pipeline behaviors (Validation, Logging, Performance)
/// 
/// Usage in Program.cs:
/// builder.Services.AddApplication();
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Application layer services to the DI container.
    /// Call this from Program.cs in the API project.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Get the assembly containing Application layer code
        // This is used to scan for handlers, validators, and profiles
        var assembly = Assembly.GetExecutingAssembly();

        // ========================================
        // Register MediatR
        // ========================================
        // MediatR scans the assembly and registers all:
        // - IRequestHandler<TRequest, TResponse> implementations (handlers)
        // - INotificationHandler<TNotification> implementations (event handlers)
        services.AddMediatR(cfg =>
        {
            // Register all handlers from this assembly
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // ========================================
        // Register FluentValidation
        // ========================================
        // Automatically finds and registers all validators in the assembly
        // Validators must inherit from AbstractValidator<T>
        services.AddValidatorsFromAssembly(assembly);

        // ========================================
        // Register AutoMapper
        // ========================================
        // Scans assembly for classes inheriting from Profile
        // We'll create mapping profiles in step 6.6
        // For now, this line is ready but won't find any profiles yet
        services.AddAutoMapper(assembly);

        // ========================================
        // Register MediatR Pipeline Behaviors
        // ========================================
        // Pipeline behaviors execute in registration order:
        // 1. Validation (fail fast if invalid)
        // 2. Logging (log valid requests)
        // 3. Performance (measure execution time)
        // Then the handler executes

        // ValidationBehavior - Automatic request validation
        // Runs all FluentValidation validators before handler
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        // LoggingBehavior - Request/response logging
        // Logs all requests with timing information
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingBehavior<,>));

        // PerformanceBehavior - Performance monitoring (optional)
        // Logs warnings for slow requests (> 500ms)
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(PerformanceBehavior<,>));

        return services;
    }
}