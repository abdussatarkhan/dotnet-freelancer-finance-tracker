using System.Text;
using System.Text.Json.Serialization;
using FreelancerTrack.Api.Middleware;
using FreelancerTrack.Application;
using FreelancerTrack.Infrastructure;
using FreelancerTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller & JSON Configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings for API readability
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

// 2. Swagger with JWT Security Definition
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Freelancer Invoicing & Expense Tracker API",
        Version = "v1",
        Description = "Production-grade Clean Architecture API for freelancers: multi-currency invoicing, milestone tracking, automated recurring retainers, expense receipts, and financial reports."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// 3. JWT Authentication Configuration
string jwtKey = builder.Configuration["Jwt:SecretKey"] ?? "FreelancerTrack_SuperSecretKey_ForDevelopmentAndProductionGradeProject_2026";
string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "FreelancerTrackApi";
string jwtAudience = builder.Configuration["Jwt:Audience"] ?? "FreelancerTrackClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 4. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 5. Clean Architecture Layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// 6. Database Migration & Realistic Seeding on Startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        logger.LogInformation("Applying PostgreSQL database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");

        logger.LogInformation("Seeding database with initial records...");
        await DatabaseSeeder.SeedAsync(context, logger);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not apply migrations automatically. Falling back to EnsureCreatedAsync...");
        try
        {
            await context.Database.EnsureCreatedAsync();
            await DatabaseSeeder.SeedAsync(context, logger);
            logger.LogInformation("Database verified and seeded via EnsureCreatedAsync.");
        }
        catch (Exception innerEx)
        {
            logger.LogError(innerEx, "PostgreSQL connection or table creation failed. Please verify PostgreSQL credentials in appsettings.json.");
        }
    }
}

// 7. HTTP Request Pipeline
app.UseMiddleware<CentralizedExceptionHandlingMiddleware>();

// Enable Swagger in all environments for testing convenience
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Freelancer Invoicing & Expense Tracker API v1");
    c.RoutePrefix = string.Empty; // Serves Swagger UI directly at application root URL "/"
});

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
