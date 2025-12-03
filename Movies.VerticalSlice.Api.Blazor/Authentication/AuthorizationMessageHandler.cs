using System.Net.Http.Headers;

namespace Movies.VerticalSlice.Api.Blazor.Authentication;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly JwtAuthenticationStateProvider _authStateProvider;

    public AuthorizationMessageHandler(JwtAuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Get the token from your auth provider
        var token = await _authStateProvider.GetTokenAsync();
        
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}