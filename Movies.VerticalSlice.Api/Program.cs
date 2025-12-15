using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Movies.VerticalSlice.Api.Configuration;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Endpoints;
using Movies.VerticalSlice.Api.Features.Movies;
using Movies.VerticalSlice.Api.Features.Ratings;
using Movies.VerticalSlice.Api.Features.Users;
using Movies.VerticalSlice.Api.Features.Logging.Create;
using Movies.VerticalSlice.Api.Features.Logging.GetAll;
using Movies.VerticalSlice.Api.Middleware;
using Movies.VerticalSlice.Api.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddHealthChecks()
//   .AddDbContextCheck<MoviesDbContext>("Database");

// Configure JWT settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

// Register services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Add services to the container.
builder.Services.AddDbContext<MoviesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings!.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Add Admin policy for administrative endpoints like logs
    // Note: This currently just requires authentication. In a production environment,
    // you should implement role-based or claim-based authorization to restrict
    // access to admin users only (e.g., by adding role claims to JWT tokens)
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireAuthenticatedUser());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    // Don't apply security globally - let endpoints control their own auth requirements
    // This way only endpoints with .RequireAuthorization() will show the padlock
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserContextService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy => policy
            .WithOrigins("https://localhost:7089") // Blazor client URL/port
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

// Use custom DB health check middleware
//app.UseDbHealthCheck();

// Now add Swagger, etc.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.MapHealthChecks("health", new HealthCheckOptions
//{
//    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
//});

app.UseHttpsRedirection();

app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();

// Add custom API request logging middleware (logs to database)
// MUST be after UseAuthentication() to have access to authenticated user
app.UseMiddleware<ApiRequestLoggingMiddleware>();

app.MapMovieEndpoints();
app.MapRatingsEndpoints();
app.MapUserEndpoints();
app.MapCreateLog();
app.MapGetAllLogs();

app.Run();









