using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Movies.VerticalSlice.Api.Blazor;
using Movies.VerticalSlice.Api.Blazor.Authentication;
using Movies.VerticalSlice.Api.Blazor.Logging;
using Movies.VerticalSlice.Api.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register JWT Auth
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddScoped<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient("AuthorizedClient", client =>
{
    var apiBaseUrl = builder.Configuration["ApiBaseUrl"] 
        ?? throw new InvalidOperationException("ApiBaseUrl is not configured in appsettings.json");
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<JwtAuthorizationMessageHandler>(); // ✅ Auto-attach token to all requests

// Add anonymous client for logging (no auth required)
builder.Services.AddHttpClient("LoggingClient", client =>
{
    var apiBaseUrl = builder.Configuration["ApiBaseUrl"] 
        ?? throw new InvalidOperationException("ApiBaseUrl is not configured in appsettings.json");
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthorizedClient"));

builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IRatingsService, RatingsService>();

// Register Telerik
builder.Services.AddTelerikBlazor();

// Configure logging to send to database
builder.Logging.SetMinimumLevel(LogLevel.Information);

var host = builder.Build();

// TEMPORARILY DISABLED: Remote logging
// Uncomment to re-enable database logging
/*
// Register remote logging after host is built
var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
var configuration = host.Services.GetRequiredService<IConfiguration>();
var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
loggerFactory.AddProvider(new RemoteLoggerProvider(httpClientFactory, configuration));
*/

await host.RunAsync();