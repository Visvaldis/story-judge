using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using StoryJudge.Api.Middleware;
using StoryJudge.Core.Interfaces;
using StoryJudge.Core.Services;
using StoryJudge.Infrastructure.Data;
using StoryJudge.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Load local configuration files (gitignored, for secrets)
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.local.json",
    optional: true,
    reloadOnChange: true);

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
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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
        options.CallbackPath = "/signin-google";
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.SaveTokens = true;
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
        options.CallbackPath = "/signin-linkedin";
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
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
app.UseCors();

// Handle forwarded headers from Cloud Run's load balancer
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<ErrorHandlingMiddleware>();

// Skip rate limiting in Testing environment
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseIpRateLimiting();
}

// Only use HTTPS redirection in development (Cloud Run handles HTTPS in production)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
