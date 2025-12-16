using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Identity;
using TaskFlow.Infrastructure.Messaging.Consumers;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.Infrastructure;

/// <summary>
/// Configures dependency injection for the Infrastructure layer.
/// This class registers all infrastructure services including:
/// - Database context and repositories
/// - Authentication and authorization services
/// - Message bus configuration (RabbitMQ with MassTransit)
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration containing connection strings and settings.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // === Database Configuration ===
        // Register Entity Framework Core with PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                // Configure retry logic for transient failures
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null
                )
            )
        );

        // === Repository Registration ===
        // Register generic repository for common CRUD operations
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Register specialized repositories for domain-specific queries
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();

        // Register Unit of Work for transaction management
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // === Authentication & Authorization Configuration ===
        // Register JWT token service for generating and validating tokens
        services.AddScoped<ITokenService, TokenService>();

        // Register authentication service for user login and registration
        services.AddScoped<IAuthService, AuthService>();

        // Register current user service for accessing authenticated user information
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Configure JWT Bearer authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured");

        services.AddAuthentication(options =>
        {
            // Set JWT Bearer as the default authentication scheme
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Configure token validation parameters
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Validate the token signature using the secret key
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

                // Validate the token issuer (who created the token)
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],

                // Validate the token audience (who the token is intended for)
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],

                // Ensure the token hasn't expired
                ValidateLifetime = true,

                // Add clock skew tolerance for time synchronization issues (default is 5 minutes)
                ClockSkew = TimeSpan.Zero
            };

            // Configure events for token authentication
            options.Events = new JwtBearerEvents
            {
                // Log authentication failures for debugging
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // === Message Bus Configuration (MassTransit + RabbitMQ) ===
        services.AddMassTransit(busConfigurator =>
        {
            // Register all message consumers from this assembly
            // Consumers listen for and process events published to RabbitMQ
            busConfigurator.AddConsumer<TaskCreatedConsumer>();
            busConfigurator.AddConsumer<TaskAssignedConsumer>();
            busConfigurator.AddConsumer<TaskStatusChangedConsumer>();

            // Configure RabbitMQ as the message transport
            busConfigurator.UsingRabbitMq((context, cfg) =>
            {
                // Get RabbitMQ connection settings from configuration
                var rabbitMqSettings = configuration.GetSection("RabbitMQ");
                var host = rabbitMqSettings["Host"] ?? "localhost";
                var virtualHost = rabbitMqSettings["VirtualHost"] ?? "/";
                var username = rabbitMqSettings["Username"] ?? "guest";
                var password = rabbitMqSettings["Password"] ?? "guest";

                // Configure RabbitMQ host connection
                cfg.Host(host, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Configure message retry policy
                // If a consumer throws an exception, MassTransit will retry processing the message
                cfg.UseMessageRetry(retryConfig =>
                {
                    // Retry up to 3 times with exponential backoff
                    retryConfig.Exponential(
                        retryLimit: 3,
                        minInterval: TimeSpan.FromSeconds(2),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(2)
                    );
                });

                // Automatically configure receive endpoints for all registered consumers
                // This creates queues in RabbitMQ for each consumer
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<INotificationService, SignalRNotificationService>();

        return services;
    }
}