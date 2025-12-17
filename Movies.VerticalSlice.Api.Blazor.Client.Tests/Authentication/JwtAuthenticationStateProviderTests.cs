using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Movies.VerticalSlice.Api.Blazor.Authentication;
using System.Security.Claims;
using Xunit;

namespace Movies.VerticalSlice.Api.Blazor.Client.Tests.Authentication;

public class JwtAuthenticationStateProviderTests : TestContext
{
    private const string ValidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
    private const string TokenKey = "authToken";

    public JwtAuthenticationStateProviderTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region GetAuthenticationStateAsync Tests

    [Fact]
    public async Task GetAuthenticationStateAsync_WithValidToken_ReturnsAuthenticatedState()
    {
        var provider = Given_we_have_a_provider_with_valid_token();
        var authState = await When_we_get_authentication_state(provider);
        Then_user_should_be_authenticated(authState);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WithNullToken_ReturnsUnauthenticatedState()
    {
        var provider = Given_we_have_a_provider_with_null_token();
        var authState = await When_we_get_authentication_state(provider);
        Then_user_should_not_be_authenticated(authState);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WithEmptyToken_ReturnsUnauthenticatedState()
    {
        var provider = Given_we_have_a_provider_with_empty_token();
        var authState = await When_we_get_authentication_state(provider);
        Then_user_should_not_be_authenticated(authState);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WithWhitespaceToken_ReturnsUnauthenticatedState()
    {
        var provider = Given_we_have_a_provider_with_whitespace_token();
        var authState = await When_we_get_authentication_state(provider);
        Then_user_should_not_be_authenticated(authState);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WithInvalidToken_ReturnsUnauthenticatedState()
    {
        var provider = Given_we_have_a_provider_with_invalid_token();
        var authState = await When_we_get_authentication_state(provider);
        Then_user_should_not_be_authenticated(authState);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WhenJSInteropThrows_ReturnsUnauthenticatedState()
    {
        var provider = Given_we_have_a_provider_that_throws_js_error();
        var authState = await When_we_get_authentication_state(provider);
        Then_user_should_not_be_authenticated(authState);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WithTokenContainingClaims_ParsesClaimsCorrectly()
    {
        var provider = Given_we_have_a_provider_with_valid_token();
        var authState = await When_we_get_authentication_state(provider);
        Then_user_should_have_correct_claims(authState);
    }

    #endregion

    #region SetTokenAsync Tests

    [Fact]
    public async Task SetTokenAsync_WithValidToken_StoresTokenInLocalStorage()
    {
        var provider = Given_we_have_a_provider_with_setup_for_setting_token();
        await When_we_set_token(provider, ValidToken);
        Then_token_should_be_stored();
    }

    [Fact]
    public async Task SetTokenAsync_WithValidToken_NotifiesAuthenticationStateChanged()
    {
        var (provider, capturedAuthState) = Given_we_have_a_provider_with_auth_state_listener();
        await When_we_set_token(provider, ValidToken);
        Then_authentication_state_changed_should_be_notified(capturedAuthState);
    }

    [Fact]
    public async Task SetTokenAsync_WithNullToken_RemovesTokenFromLocalStorage()
    {
        var provider = Given_we_have_a_provider_with_setup_for_removing_token();
        await When_we_set_token(provider, null!);
        Then_token_should_be_removed();
    }

    [Fact]
    public async Task SetTokenAsync_WithEmptyToken_RemovesTokenFromLocalStorage()
    {
        var provider = Given_we_have_a_provider_with_setup_for_removing_token();
        await When_we_set_token(provider, string.Empty);
        Then_token_should_be_removed();
    }

    [Fact]
    public async Task SetTokenAsync_WithWhitespaceToken_RemovesTokenFromLocalStorage()
    {
        var provider = Given_we_have_a_provider_with_setup_for_removing_token();
        await When_we_set_token(provider, "   ");
        Then_token_should_be_removed();
    }

    [Fact]
    public async Task SetTokenAsync_WhenJSInteropThrows_ThrowsException()
    {
        var provider = Given_we_have_a_provider_that_throws_on_set_token();
        await Then_setting_token_should_throw_exception(provider);
    }

    [Fact]
    public async Task SetTokenAsync_CalledMultipleTimes_NotifiesEachTime()
    {
        var (provider, notificationCount) = Given_we_have_a_provider_with_notification_counter();
        await When_we_set_token_multiple_times(provider);
        Then_should_notify_multiple_times(notificationCount);
    }

    #endregion

    #region GetTokenAsync Tests

    [Fact]
    public async Task GetTokenAsync_WithStoredToken_ReturnsToken()
    {
        var provider = Given_we_have_a_provider_with_valid_token();
        var token = await When_we_get_token(provider);
        Then_token_should_be_returned(token);
    }

    [Fact]
    public async Task GetTokenAsync_WithNoStoredToken_ReturnsNull()
    {
        var provider = Given_we_have_a_provider_with_null_token();
        var token = await When_we_get_token(provider);
        Then_token_should_be_null(token);
    }

    [Fact]
    public async Task GetTokenAsync_WhenJSInteropThrows_ReturnsNull()
    {
        var provider = Given_we_have_a_provider_that_throws_js_error();
        var token = await When_we_get_token(provider);
        Then_token_should_be_null(token);
    }

    #endregion

    #region Given Methods

    JwtAuthenticationStateProvider Given_we_have_a_provider_with_valid_token()
    {
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult(ValidToken);
        return new JwtAuthenticationStateProvider(JSInterop.JSRuntime);
    }

    JwtAuthenticationStateProvider Given_we_have_a_provider_with_null_token()
    {
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult((string)null!);
        return new JwtAuthenticationStateProvider(JSInterop.JSRuntime);
    }

    JwtAuthenticationStateProvider Given_we_have_a_provider_with_empty_token()
    {
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult(string.Empty);
        return new JwtAuthenticationStateProvider(JSInterop.JSRuntime);
    }

    JwtAuthenticationStateProvider Given_we_have_a_provider_with_whitespace_token()
    {
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult("   ");
        return new JwtAuthenticationStateProvider(JSInterop.JSRuntime);
    }

    JwtAuthenticationStateProvider Given_we_have_a_provider_with_invalid_token()
    {
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult("invalid.token.format");
        return new JwtAuthenticationStateProvider(JSInterop.JSRuntime);
    }

    JwtAuthenticationStateProvider Given_we_have_a_provider_that_throws_js_error()
    {
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetException(new InvalidOperationException("JS runtime error"));
        return new JwtAuthenticationStateProvider(JSInterop.JSRuntime);
    }

    JwtAuthenticationStateProvider Given_we_have_a_provider_with_setup_for_setting_token()
    {
        JSInterop.SetupVoid("localStorage.setItem", TokenKey, ValidToken)
            .SetVoidResult();
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult(ValidToken);
        return new JwtAuthenticationStateProvider(JSInterop.JSRuntime);
    }

    JwtAuthenticationStateProvider Given_we_have_a_provider_with_setup_for_removing_token()
    {
        JSInterop.SetupVoid("localStorage.removeItem", TokenKey)
            .SetVoidResult();
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult((string)null!);
        return new JwtAuthenticationStateProvider(JSInterop.JSRuntime);
    }

    JwtAuthenticationStateProvider Given_we_have_a_provider_that_throws_on_set_token()
    {
        JSInterop.SetupVoid("localStorage.setItem", TokenKey, ValidToken)
            .SetException(new InvalidOperationException("JS runtime error"));
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult((string)null!);
        return new JwtAuthenticationStateProvider(JSInterop.JSRuntime);
    }

    (JwtAuthenticationStateProvider provider, AuthStateCapture capturedAuthState) Given_we_have_a_provider_with_auth_state_listener()
    {
        JSInterop.SetupVoid("localStorage.setItem", TokenKey, ValidToken)
            .SetVoidResult();
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult(ValidToken);

        var provider = new JwtAuthenticationStateProvider(JSInterop.JSRuntime);
        var capturedAuthState = new AuthStateCapture();

        provider.AuthenticationStateChanged += task =>
        {
            capturedAuthState.WasCalled = true;
        };

        return (provider, capturedAuthState);
    }

    (JwtAuthenticationStateProvider provider, NotificationCounter counter) Given_we_have_a_provider_with_notification_counter()
    {
        JSInterop.SetupVoid("localStorage.setItem", TokenKey, ValidToken)
            .SetVoidResult();
        JSInterop.SetupVoid("localStorage.removeItem", TokenKey)
            .SetVoidResult();
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult(ValidToken);

        var provider = new JwtAuthenticationStateProvider(JSInterop.JSRuntime);
        var counter = new NotificationCounter();

        provider.AuthenticationStateChanged += _ =>
        {
            counter.Count++;
        };

        return (provider, counter);
    }

    #endregion

    #region When Methods

    async Task<AuthenticationState> When_we_get_authentication_state(JwtAuthenticationStateProvider provider)
    {
        return await provider.GetAuthenticationStateAsync();
    }

    async Task When_we_set_token(JwtAuthenticationStateProvider provider, string token)
    {
        await provider.SetTokenAsync(token);
    }

    async Task<string?> When_we_get_token(JwtAuthenticationStateProvider provider)
    {
        return await provider.GetTokenAsync();
    }

    async Task When_we_set_token_multiple_times(JwtAuthenticationStateProvider provider)
    {
        await provider.SetTokenAsync(ValidToken);
        await provider.SetTokenAsync(null!);
    }

    #endregion

    #region Then Methods

    void Then_user_should_be_authenticated(AuthenticationState authState)
    {
        Assert.NotNull(authState);
        Assert.True(authState.User.Identity?.IsAuthenticated);
        Assert.Equal("jwt", authState.User.Identity?.AuthenticationType);
    }

    void Then_user_should_not_be_authenticated(AuthenticationState authState)
    {
        Assert.NotNull(authState);
        Assert.False(authState.User.Identity?.IsAuthenticated);
    }

    void Then_user_should_have_correct_claims(AuthenticationState authState)
    {
        Assert.NotNull(authState);
        Assert.True(authState.User.Identity?.IsAuthenticated);
        Assert.Contains(authState.User.Claims, c => c.Type == "sub" && c.Value == "1234567890");
        Assert.Contains(authState.User.Claims, c => c.Type == "name" && c.Value == "John Doe");
    }

    void Then_token_should_be_stored()
    {
        JSInterop.VerifyInvoke("localStorage.setItem");
    }

    void Then_token_should_be_removed()
    {
        JSInterop.VerifyInvoke("localStorage.removeItem");
    }

    void Then_authentication_state_changed_should_be_notified(AuthStateCapture capturedAuthState)
    {
        Assert.True(capturedAuthState.WasCalled);
    }

    async Task Then_setting_token_should_throw_exception(JwtAuthenticationStateProvider provider)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await provider.SetTokenAsync(ValidToken));
    }

    void Then_token_should_be_returned(string? token)
    {
        Assert.Equal(ValidToken, token);
    }

    void Then_token_should_be_null(string? token)
    {
        Assert.Null(token);
    }

    void Then_should_notify_multiple_times(NotificationCounter counter)
    {
        Assert.Equal(2, counter.Count);
    }

    #endregion

    #region Helper Classes

    private class AuthStateCapture
    {
        public bool WasCalled { get; set; }
    }

    private class NotificationCounter
    {
        public int Count { get; set; }
    }

    #endregion
}
