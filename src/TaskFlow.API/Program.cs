using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
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
builder.Services.AddInfrastructure();

// ========================================
// Configure JWT Authentication
// ========================================
// Add HttpContextAccessor (needed by CurrentUserService)
builder.Services.AddHttpContextAccessor();

// Configure JWT Bearer authentication
builder.Services.AddAuthentication(options =>
{
    // Use JWT Bearer as the default authentication scheme
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Read JWT settings from appsettings.json
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured");

    // Configure token validation parameters
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validate the token signature (prevents tampering)
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

        // Validate the issuer (who created the token)
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],

        // Validate the audience (who the token is for)
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],

        // Validate the token hasn't expired
        ValidateLifetime = true,

        // No clock skew (token expires exactly at expiration time)
        ClockSkew = TimeSpan.Zero
    };

    // Optional: Configure events for logging/debugging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // Log authentication failures
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Log when authentication is challenged (401 response)
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // Log successful token validation
            return Task.CompletedTask;
        }
    };
});

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

app.UseHttpsRedirection();

// ========================================
// Authentication & Authorization Middleware
// ========================================
// IMPORTANT: Order matters!
// 1. UseAuthentication - Validates JWT token
// 2. UseAuthorization - Checks permissions
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();