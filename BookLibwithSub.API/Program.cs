using System;
using System.Text;
using BookLibwithSub.API.Security;
using BookLibwithSub.Repo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------
// Services
// -------------------------------
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
const string CorsPolicy = "AppCors";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(o => o.AddPolicy(
    CorsPolicy,
    p => p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()
));

// -------------------------------
// JWT bootstrap (no length checks)
// -------------------------------
string jwtIssuer = builder.Configuration["Jwt:Issuer"]
                   ?? Environment.GetEnvironmentVariable("JWT__ISSUER")
                   ?? "BookLibIssuer";

string configuredKey = builder.Configuration["Jwt:Key"]
                       ?? Environment.GetEnvironmentVariable("JWT__KEY");

// fail fast if key missing outside Dev
if (string.IsNullOrWhiteSpace(configuredKey) && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "JWT signing key is not configured. Set env var JWT__KEY or configuration Jwt:Key.");
}

// Build signing key (prefer Base64; else UTF8). No size validation.
static SymmetricSecurityKey BuildSigningKey(string key)
{
    key ??= string.Empty;
    var trimmed = key.Trim();
    try
    {
        return new SymmetricSecurityKey(Convert.FromBase64String(trimmed));
    }
    catch (FormatException)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(trimmed));
    }
}

var signingKey = BuildSigningKey(configuredKey);
builder.Services.AddSingleton(signingKey); // reuse in TokenService

// AuthN/AuthZ
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = false, // no audience configured
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("LibrarianOrAdmin", p => p.RequireRole("Librarian", "Admin"));
});

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BookLibWithSub API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT as: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });
});

var app = builder.Build();

// -------------------------------
// Pipeline
// -------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookLibWithSub API v1");
});

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "BookLibWithSub API is running");

app.Run();
