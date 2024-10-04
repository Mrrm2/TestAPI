using System.Text;
using Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using TestAPI.Attributes;
using TestAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMemoryCache(); // Register IMemoryCache // For Cache

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Set Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SampleDbContext>(options =>
    options.UseSqlite(connectionString));


builder.Services.AddControllers().AddNewtonsoftJson(options =>
  options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);

builder.Services.AddAuthorization();

// Add Identity requirements
builder.Services.AddIdentity<IdentityUser, IdentityRole>(
	option =>
	{
		option.Password.RequireDigit = false;
		option.Password.RequiredLength = 6;
		option.Password.RequireNonAlphanumeric = false;
		option.Password.RequireUppercase = false;
		option.Password.RequireLowercase = false;
	}
).AddEntityFrameworkStores<SampleDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
builder.Services.AddAuthentication(option => {
  option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
  options.SaveToken = true;
  options.RequireHttpsMetadata = true;
  options.TokenValidationParameters = new TokenValidationParameters()
  {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidAudience = builder.Configuration["Jwt:Site"],
      ValidIssuer = builder.Configuration["Jwt:Site"],
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!))
  };
});

// custom header for test purposes
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<AddHeaderOperationFilter>(); 
});

var app = builder.Build();

// Set the custom attribute cache
InterceptOutbound.SetCache(app.Services.GetRequiredService<IMemoryCache>());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();
