using IronLogic.Application.Interfaces;
using IronLogic.Application.Mappers;
using IronLogic.Application.Services;
using IronLogic.Domain.Interfaces;
using IronLogic.Infrastructure;
using IronLogic.Infrastructure.Data;
using IronLogic.Infrastructure.Repositories;
using IronLogic.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel; // Added for Kernel registration

var builder = WebApplication.CreateBuilder(args);

// 1. Add API configurations
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "IronLogic API";
    config.Title = "IronLogic AI API";
    config.Version = "v1";
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=ironlogic.db";

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlite(connectionString)
        .EnableSensitiveDataLogging(false));

builder.Services.AddSingleton<IHevyParserService, HevyCsvParserService>();
builder.Services.AddSingleton<IHevyDataMapper, HevyDataMapper>();

builder.Services.AddScoped<WorkoutAnalysisService>();
builder.Services.AddScoped<IronLogicCoachService>();
builder.Services.AddScoped<IDailyWeightService, DailyWeightService>();
builder.Services.AddScoped<IMuscleMeasurementService, MuscleMeasurementService>();
builder.Services.AddScoped<IWorkoutSessionRepository, WorkoutSessionRepository>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();

// Register Kernel (concrete) for Semantic Kernel usage across services/controllers.
// NOTE: If you want to configure a chat provider (OpenAI, AzureOpenAI, etc.) add provider configuration
// using the Kernel builder here (example the project owner suggested:
// Kernel.CreateBuilder().AddOpenAIChatCompletion(...).Build(); )
builder.Services.AddScoped<Kernel>(_ => Kernel.CreateBuilder().Build());

// -----------------------------------------------------------

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}
// -----------------------------------------------------------

// 2. Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/health", () => "IronLogic API is running perfectly! ??");

app.Run();

// Make the implicit Program class public so integration tests can reference it via WebApplicationFactory<Program>
public partial class Program;