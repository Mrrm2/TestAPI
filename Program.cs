using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TestAPI.Data;
using TestAPI.IdempotencyLibrary.Filters;
using TestAPI.IdempotencyLibrary.Implementations;
using TestAPI.IdempotencyLibrary.Interfaces;

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
            options.RequireHttpsMetadata = false;
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
            // options.Authority = "https://TransLink-auth-server.com"; // Replace with actual authority URL when available

            // ********************************************************************************************************************
            options.Authority = "https://Azure-mock-auth-testing.com"; // Replace with mock authority URL for testing when setup
            // ********************************************************************************************************************

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

// Add Swagger for API testing during development
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
