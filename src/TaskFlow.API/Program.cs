using Microsoft.EntityFrameworkCore;
using TaskFlow.Application;
using TaskFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ========================================
// Configure Database Context
// ========================================
// Register ApplicationDbContext with dependency injection
// This tells EF Core to use PostgreSQL with our connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Get connection string from appsettings.json
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // Configure to use PostgreSQL with Npgsql provider
    options.UseNpgsql(connectionString);

    // Enable sensitive data logging in development for debugging
    // Shows parameter values in SQL logs (useful for learning)
    // WARNING: Don't enable in production (security risk)
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

// ========================================
// Register Application Layer Services
// ========================================
// This registers MediatR, FluentValidation, AutoMapper, and Pipeline Behaviors
// All application logic services are configured in one call
builder.Services.AddApplication();

// ========================================
// Register API Services
// ========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();