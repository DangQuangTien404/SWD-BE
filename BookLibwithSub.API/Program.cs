using System;
using System.Text;
using System.Text.Json.Serialization;
using BookLibwithSub.API.Middleware;
using BookLibwithSub.Repo;
using BookLibwithSub.Repo.Interfaces;
using BookLibwithSub.Repo.repository;
using BookLibwithSub.Service.Interfaces;
using BookLibwithSub.Service.Models;
using BookLibwithSub.Service.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
builder.Services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IBookService, BookService>();

builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IBookRepository, BookRepository>();


builder.Services.AddHttpClient();
builder.Services.AddScoped<IZaloPayService, ZaloPayService>();
builder.Services.Configure<ZaloPayOptions>(builder.Configuration.GetSection("ZaloPay"));


const string CorsPolicy = "AppCors";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[]
{
    "http://localhost:5173",
    "http://localhost:3000",
    "https://swd392-booklibwithsub-fe.vercel.app"
};

builder.Services.AddCors(o =>
{
    o.AddPolicy(CorsPolicy, p =>
    {
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials(); 
    });
});


var jwtOptions = new JwtOptions();
builder.Configuration.GetSection("Jwt").Bind(jwtOptions);
jwtOptions.Key = Environment.GetEnvironmentVariable("JWT__KEY") ?? jwtOptions.Key;
jwtOptions.Issuer = Environment.GetEnvironmentVariable("JWT__ISSUER") ?? jwtOptions.Issuer;

if (string.IsNullOrWhiteSpace(jwtOptions.Key) && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "JWT signing key is not configured. Set env var JWT__KEY or configuration Jwt:Key.");
}

builder.Services.Configure<JwtOptions>(o =>
{
    o.Key = jwtOptions.Key;
    o.Issuer = jwtOptions.Issuer;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key ?? string.Empty))
        };
    });

builder.Services.AddAuthorization();


builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});


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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookLibWithSub API v1");
    });
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseMiddleware<TokenValidationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.MapMethods("{*path}", new[] { "OPTIONS" }, () => Results.Ok());

app.MapGet("/", () => "BookLibWithSub API is running");

app.Run();
