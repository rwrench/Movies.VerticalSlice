using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Movies.VerticalSlice.Api.Blazor.Authentication;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private const string TokenKey = "authToken";

    public JwtAuthenticationStateProvider(IJSRuntime js)
    {
        _js = js;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
            var identity = string.IsNullOrWhiteSpace(token)
                ? new ClaimsIdentity()
                : new ClaimsIdentity(new JwtSecurityTokenHandler().ReadJwtToken(token).Claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch (Exception)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public virtual async Task SetTokenAsync(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
            }

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch (Exception)
        {
            // Silent fail
            throw;
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
