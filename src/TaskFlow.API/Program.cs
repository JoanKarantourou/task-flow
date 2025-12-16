using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskFlow.API.Hubs;
using TaskFlow.Application;
using TaskFlow.Infrastructure;
using TaskFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ========================================
// Configure Database Context
// ========================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

// ========================================
// Register Application Layer Services
// ========================================
// Registers MediatR, FluentValidation, AutoMapper, and Pipeline Behaviors
builder.Services.AddApplication();

// ========================================
// Register Infrastructure Layer Services
// ========================================
// Registers UnitOfWork, Repositories, and Authentication Services
builder.Services.AddInfrastructure(builder.Configuration);

// ========================================
// Configure HTTP Context Accessor
// ========================================
// Add HttpContextAccessor (needed by CurrentUserService)
builder.Services.AddHttpContextAccessor();

// ========================================
// Register API Services
// ========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ========================================
// Configure Swagger with JWT Support
// ========================================
builder.Services.AddSwaggerGen(options =>
{
    // Add API info
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskFlow API",
        Version = "v1",
        Description = "A task management platform API with JWT authentication",
        Contact = new OpenApiContact
        {
            Name = "Joan Karantourou",
            Url = new Uri("https://github.com/JoanKarantourou")
        }
    });

    // Add JWT Bearer authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the text input below.\n\nExample: \"eyJhbGciOiJIUzI1NiIs...\""
    });

    // Make all endpoints require JWT authentication by default
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ========================================
// Configure SignalR
// ========================================
builder.Services.AddSignalR(options =>
{
    // Enable detailed errors in development for debugging
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();

    // Configure timeouts
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);

    // Configure message size limits (increase if sending large notifications)
    options.MaximumReceiveMessageSize = 32 * 1024; // 32 KB
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API V1");
        options.RoutePrefix = string.Empty; // Serve Swagger at root (http://localhost:5000/)
    });
}

// Disable HTTPS redirection in development to allow HTTP SignalR connections
// app.UseHttpsRedirection();

// ========================================
// Enable WebSockets for SignalR
// ========================================
app.UseWebSockets();

// ========================================
// Authentication & Authorization Middleware
// ========================================
// IMPORTANT: Order matters!
// 1. UseAuthentication - Validates JWT token
// 2. UseAuthorization - Checks permissions
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ========================================
// Map SignalR Hub Endpoint
// ========================================
// This creates the endpoint /hubs/notifications that clients connect to
// Requires authentication - only authenticated users can connect
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();