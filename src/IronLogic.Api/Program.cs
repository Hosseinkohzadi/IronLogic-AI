using IronLogic.Application.Interfaces;
using IronLogic.Application.Mappers;
using IronLogic.Application.Services;
using IronLogic.Infrastructure;
using IronLogic.Infrastructure.Data;
using IronLogic.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

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

app.MapGet("/api/health", () => "IronLogic API is running perfectly! 🚀");

app.Run();