using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Movies.VerticalSlice.Api.Blazor;
using Movies.VerticalSlice.Api.Blazor.Authentication;
using Telerik.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://your-api-url/") });

// Register Telerik
builder.Services.AddTelerikBlazor();

// Register JWT Auth
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

builder.Services.AddTransient<JwtAuthorizationMessageHandler>();
builder.Services.AddScoped(sp =>
{
    var js = sp.GetRequiredService<IJSRuntime>();
    var handler = sp.GetRequiredService<JwtAuthorizationMessageHandler>();
    return new HttpClient(handler) { BaseAddress = new Uri("https://your-api-url/") };
});

await builder.Build().RunAsync();