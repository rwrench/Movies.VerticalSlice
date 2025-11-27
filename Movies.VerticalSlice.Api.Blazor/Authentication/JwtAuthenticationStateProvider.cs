using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Movies.VerticalSlice.Api.Blazor.Authentication;


public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private const string TokenKey = "authToken";

    public JwtAuthenticationStateProvider(IJSRuntime js) => _js = js;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
        var identity = string.IsNullOrWhiteSpace(token)
            ? new ClaimsIdentity()
            : new ClaimsIdentity(new JwtSecurityTokenHandler().ReadJwtToken(token).Claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public virtual async Task SetTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        else
            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
