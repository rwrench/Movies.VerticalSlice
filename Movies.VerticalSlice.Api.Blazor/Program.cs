using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Movies.VerticalSlice.Api.Blazor;
using Movies.VerticalSlice.Api.Blazor.Authentication;
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
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7299");
})
.AddHttpMessageHandler<JwtAuthorizationMessageHandler>(); // ✅ Auto-attach token to all requests

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthorizedClient"));

builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IRatingsService, RatingsService>();

// Register Telerik
builder.Services.AddTelerikBlazor();

await builder.Build().RunAsync();