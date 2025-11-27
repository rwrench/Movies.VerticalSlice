using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Movies.VerticalSlice.Api.Blazor.Authentication;
using Movies.VerticalSlice.Api.Blazor.Client.Tests.Mocks;
using Movies.VerticalSlice.Api.Blazor.Client.Tests.TestHelpers;
using Movies.VerticalSlice.Api.Blazor.Pages;
using Movies.VerticalSlice.Api.Shared.Responses;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Movies.VerticalSlice.Api.Blazor.Client.Tests.Page;
public partial class LoginPageTests : BunitContext
{
    [Fact]
    public void Login_SetsToken()
    {
        (IRenderedComponent<Pages.Login> cut, TestJwtAuthenticationStateProvider auth) 
            = Given_we_have_a_login_page();
        When_we_click_login(cut);
        Then_we_should_get_the_token_set(cut, auth); 
    }

    [Fact]
    public void Login_Navigates_to_root()
    {
        (IRenderedComponent<Pages.Login> cut, TestJwtAuthenticationStateProvider auth)
            = Given_we_have_a_login_page();
        When_we_click_login(cut);
        Then_we_should_navigate_to_the_root(cut);
    }

    [Fact]
    public void Login_Failure_ShowsError()
    {
        var (cut, auth, _) = RenderWithHandler(HttpStatusCode.BadRequest);
        When_we_click_login(cut);
        Then_we_should_show_error(cut, auth);
    }
    [Fact]
    public void Login_Success_Does_Not_Show_Error()
    {
        (IRenderedComponent<Pages.Login> cut, TestJwtAuthenticationStateProvider auth)
            = Given_we_have_a_login_page();
        When_we_click_login(cut);
        Then_we_should_NOT_show_an_error(cut, auth);
       
    }

   
    (IRenderedComponent<Pages.Login> cut, TestJwtAuthenticationStateProvider auth)
       Given_we_have_a_login_page()
    {
        var (cut, auth, _) = RenderWithHandler(HttpStatusCode.OK,
            new LoginResult { Token = "valid_token" });

        JSInterop.SetupVoid("localStorage.setItem", "authToken", "valid_token");
        return (cut, auth);

    }
    void When_we_click_login(IRenderedComponent<Pages.Login> cut)
    {
        cut.Find("button").Click();
    }

    void Then_we_should_navigate_to_the_root(IRenderedComponent<Pages.Login> cut)
    {
        cut.WaitForAssertion(() =>
        {
            Assert.EndsWith("/", Services.GetRequiredService<NavigationManager>().Uri);
        });
    }

    void Then_we_should_get_the_token_set(
        IRenderedComponent<Pages.Login> cut,
        TestJwtAuthenticationStateProvider auth)
    {
        cut.WaitForAssertion(() =>
        {
            Assert.True(auth.SetTokenCalled);
            Assert.Equal("valid_token", auth.LastToken);
        });
    }

    TestJwtAuthenticationStateProvider RegisterAuth()
    {
        var auth = new TestJwtAuthenticationStateProvider(JSInterop.JSRuntime);
        Services.AddSingleton<JwtAuthenticationStateProvider>(auth);
        return auth;
    }

    (
        IRenderedComponent<Pages.Login> cut,
        TestJwtAuthenticationStateProvider auth,
        CapturingHttpHandler handler)
        RenderWithHandler(HttpStatusCode code, LoginResult? result = null)
    {
        var handler = new CapturingHttpHandler(req =>
        {
            var response = new HttpResponseMessage(code)
            {
                Content = result is not null ? JsonContent.Create(result) : null
            };
            return Task.FromResult(response);
        });

        Services.AddSingleton<IHttpClientFactory>(
            new MockHttpClientFactory(handler, new Uri("https://example.test/")));
        var auth = RegisterAuth();
        var cut = Render<Pages.Login>();
        return (cut, auth, handler);
    }

    void Then_we_should_show_error(
        IRenderedComponent<Pages.Login> cut,
        TestJwtAuthenticationStateProvider auth)
    {
        cut.WaitForAssertion(() =>
        {
            Assert.False(auth.SetTokenCalled);
            Assert.Contains("Invalid login", cut.Markup);
        });
    }

    void Then_we_should_NOT_show_an_error(
       IRenderedComponent<Login> cut,
       TestJwtAuthenticationStateProvider auth)
    {
        cut.WaitForAssertion(() =>
        {
            Assert.DoesNotContain("Invalid login", cut.Markup);
            Assert.True(auth.SetTokenCalled);
            Assert.Equal("valid_token", auth.LastToken);
        });
    }



}
