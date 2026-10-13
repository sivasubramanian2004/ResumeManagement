using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ResumeManagement.Api;
using ResumeManagement.Api.Extensions;
using ResumeManagement.Api.Middleware;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// =====================================
// 1. SERILOG CONFIGURATION
// =====================================

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Information()
    .MinimumLevel.Override(
        "Microsoft.AspNetCore",
        Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override(
        "Microsoft",
        Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 10,
        outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// =====================================
// 2. DATABASE CONFIGURATION
// =====================================

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "DefaultConnection is missing from appsettings.json.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(connectionString));

// =====================================
// 3. APPLICATION SERVICES
// =====================================

builder.Services.AddApplicationServices(builder.Configuration);

// =====================================
// 4. CONTROLLERS + JSON ENUMS
// =====================================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

// =====================================
// 5. OPENAPI
// =====================================

builder.Services.AddOpenApi();

// =====================================
// 6. CORS
// =====================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// =====================================
// 7. BUILD APPLICATION
// =====================================

var app = builder.Build();

// =====================================
// 8. OPENAPI + SWAGGER
// =====================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

// =====================================
// 9. MIDDLEWARE
// =====================================
app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.UseOutputCache();

// =====================================
// 10. CONTROLLERS
// =====================================

app.MapControllers();

// =====================================
// 11. HEALTH ENDPOINT
// =====================================

app.MapGet("/health", () =>
    Results.Ok("Healthy"))
    .WithName("HealthCheck")
    .WithTags("Health")
    .WithSummary("Checks whether the API is responding");

// =====================================
// 12. RUN APPLICATION
// =====================================

try
{
    Log.Information("Starting Resume Management API...");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}