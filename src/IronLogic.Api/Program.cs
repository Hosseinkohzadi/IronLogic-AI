var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Register the NSwag services
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "IronLogic API";
    config.Title = "IronLogic AI API";
    config.Version = "v1";
    config.Description = "The core backend service for the IronLogic Fitness App";
});

var app = builder.Build();

// 2. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Add OpenAPI 3.0 document serving middleware
    app.UseOpenApi();

    // Add Swagger UI routing
    app.UseSwaggerUi(); // Serves UI at /swagger
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// A simple health check endpoint
app.MapGet("/api/health", () => "IronLogic API is running perfectly! 🚀");

app.Run();