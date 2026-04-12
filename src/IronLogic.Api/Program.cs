using IronLogic.Api;
using IronLogic.Api.Middleware;
using IronLogic.Infrastructure;
using IronLogic.Infrastructure.Data;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using IronLogic.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IronLogic.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // Allow sign-in with email
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddSignInManager<SignInManager<User>>();

var jwtKey = builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("JWT Key is missing in configuration!");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowIronLogicDash", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:5011")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddMemoryCache();
builder.Services.AddHangfire(configuration => configuration.UseMemoryStorage());
builder.Services.AddHangfireServer();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "IronLogic API";
    config.Title = "IronLogic AI API";
    config.Version = "v1";

    // Add JWT Authentication to Swagger
    config.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Please enter a valid JWT token in the format: Bearer {token}"
    });

    config.OperationProcessors.Add(new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});

builder.Services
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddSemanticKernel(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

        try
        {
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("Starting database initialization...");

            // Step 1: Ensure database is created/migrated
            dbContext.Database.Migrate();
            logger.LogInformation("Database migration completed successfully");

            // Step 2, 3 & 4: Ensure roles exist, seed admin user and exercises
            await ExerciseSeederService.SeedAsync(dbContext, userManager, roleManager, loggerFactory);

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseRouting();

app.UseCors("AllowIronLogicDash");

app.UseHttpsRedirection();

app.UseGlobalExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
EmailJobsBootstrapper.Register(recurringJobManager);

app.MapControllers();

app.MapGet("/api/health", () => "IronLogic API is running perfectly! 🏋️");

app.Run();