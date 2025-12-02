using Microsoft.EntityFrameworkCore;
using TaskFlow.Application;
using TaskFlow.Infrastructure;
using TaskFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ========================================
// Configure Database Context
// ========================================
// Register ApplicationDbContext with dependency injection
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
// Registers UnitOfWork and Repository implementations
builder.Services.AddInfrastructure();

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