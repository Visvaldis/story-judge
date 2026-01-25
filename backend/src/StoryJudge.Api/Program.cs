using System.Security.Claims;
using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using StoryJudge.Api.Middleware;
using StoryJudge.Core.Interfaces;
using StoryJudge.Core.Services;
using StoryJudge.Infrastructure.Data;
using StoryJudge.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// MongoDB Configuration
var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"]
    ?? throw new InvalidOperationException("MongoDB:ConnectionString is not configured");
var mongoDatabaseName = builder.Configuration["MongoDB:DatabaseName"]
    ?? throw new InvalidOperationException("MongoDB:DatabaseName is not configured");

builder.Services.AddSingleton(_ => new MongoDbContext(mongoConnectionString, mongoDatabaseName));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStoryRepository, StoryRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IReportService, ReportService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "StoryJudge";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "StoryJudge";

var authBuilder = builder.Services.AddAuthentication(options =>
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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Only add OAuth providers if credentials are configured
var googleClientId = builder.Configuration["OAuth:Google:ClientId"];
var googleClientSecret = builder.Configuration["OAuth:Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/api/auth/callback/google";
    });
}

var linkedInClientId = builder.Configuration["OAuth:LinkedIn:ClientId"];
var linkedInClientSecret = builder.Configuration["OAuth:LinkedIn:ClientSecret"];
if (!string.IsNullOrEmpty(linkedInClientId) && !string.IsNullOrEmpty(linkedInClientSecret))
{
    authBuilder.AddLinkedIn(options =>
    {
        options.ClientId = linkedInClientId;
        options.ClientSecret = linkedInClientSecret;
        options.CallbackPath = "/api/auth/callback/linkedin";
    });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));
    options.AddPolicy("Moderator", policy => policy.RequireClaim(ClaimTypes.Role, "Admin", "Moderator"));
});

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// CORS - Environment variable takes precedence for production deployment
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Environment variable takes precedence (for Cloud Run), then config file
        // Use IsNullOrWhiteSpace to handle empty string case
        var envOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
        var originsConfig = !string.IsNullOrWhiteSpace(envOrigins)
            ? envOrigins
            : (builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:5173");

        var allowedOrigins = originsConfig
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        // Log for debugging
        Console.WriteLine($"[CORS] Environment variable: '{envOrigins ?? "(null)"}'");
        Console.WriteLine($"[CORS] Allowed Origins: {string.Join(", ", allowedOrigins)}");

        if (allowedOrigins.Length == 0)
        {
            Console.WriteLine("[CORS] WARNING: No origins configured! Using wildcard for debugging.");
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS must be first to handle preflight requests
app.UseCors("AllowFrontend");

// Handle forwarded headers from Cloud Run's load balancer
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseIpRateLimiting();

// Only use HTTPS redirection in development (Cloud Run handles HTTPS in production)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
