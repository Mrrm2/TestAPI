using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TestAPI.Data;
using TestAPI.IdempotencyLibrary.Filters;
using TestAPI.IdempotencyLibrary.Implementations;
using TestAPI.IdempotencyLibrary.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SampleDbContext>(options =>
    options.UseSqlite(connectionString));

// Register Controllers with Idempotency Filter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<IdempotencyFilter>(); // Register the Idempotency Filter globally for all controllers
});

// Register Idempotency Store
builder.Services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>(); // Registers the in-memory idempotency store

// Register Authentication Services (OAuth2)
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false; // Skips HTTPS validation during local testing
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("UqY=w!RhCg&7@WxHudn`9ac8BbGFQ:y>")), // Example key for testing - move to environment variable in production
                ValidateIssuerSigningKey = true
            };
        }
        else
        {
            options.Authority = "https://TransLink-auth-server.com"; // Replace with actual authority URL when available

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false
            };
        }
    });

// Register Authorization Services
builder.Services.AddAuthorization();

// Register Logging Services
builder.Services.AddLogging(configure => configure.AddConsole().AddDebug());

// Add Swagger for API testing during development, including the Idempotency-Key header
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TestAPI", Version = "v1" });
    
    // Add custom header parameter for Idempotency-Key to Swagger
    c.OperationFilter<AddRequiredHeaderParameter>();
});

var app = builder.Build();

// Mock OAuth2 Middleware for Testing (only for development)
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        // Mock a user for development purposes
        var claims = new List<Claim>
        {
            new Claim("client_id", "mock-client-id"),
            new Claim(ClaimTypes.Name, "Test User")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        await next();
    });

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Custom operation filter to add Idempotency-Key header to Swagger UI
public class AddRequiredHeaderParameter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Idempotency-Key",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string"
            },
            Description = "Idempotency Key for ensuring request idempotency"
        });
    }
}