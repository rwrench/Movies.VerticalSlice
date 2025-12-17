using Bunit;
using Microsoft.JSInterop;
using Movies.VerticalSlice.Api.Blazor.Authentication;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Movies.VerticalSlice.Api.Blazor.Client.Tests.Authentication;

public class JwtAuthorizationMessageHandlerTests : TestContext
{
    private const string ValidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
    private const string TokenKey = "authToken";

    #region Token Attachment Tests

    [Fact]
    public async Task SendAsync_WithStoredToken_AttachesTokenToRequest()
    {
        var handler = Given_we_have_a_handler_with_valid_token();
        var request = And_we_have_a_request();
        await When_we_send_request(handler, request);
        Then_token_should_be_attached_to_request(request);
    }

    [Fact]
    public async Task SendAsync_WithNoStoredToken_DoesNotAttachToken()
    {
        var handler = Given_we_have_a_handler_with_no_token();
        var request = And_we_have_a_request();
        await When_we_send_request(handler, request);
        Then_token_should_not_be_attached(request);
    }

    [Fact]
    public async Task SendAsync_WithEmptyToken_DoesNotAttachToken()
    {
        var handler = Given_we_have_a_handler_with_empty_token();
        var request = And_we_have_a_request();
        await When_we_send_request(handler, request);
        Then_token_should_not_be_attached(request);
    }

    [Fact]
    public async Task SendAsync_WithWhitespaceToken_DoesNotAttachToken()
    {
        var handler = Given_we_have_a_handler_with_whitespace_token();
        var request = And_we_have_a_request();
        await When_we_send_request(handler, request);
        Then_token_should_not_be_attached(request);
    }

    [Fact]
    public async Task SendAsync_WithExistingAuthorizationHeader_OverridesWithToken()
    {
        var handler = Given_we_have_a_handler_with_valid_token();
        var request = And_we_have_a_request_with_existing_auth_header();
        await When_we_send_request(handler, request);
        Then_token_should_be_overridden(request);
    }

    #endregion

    #region Handler Invocation Tests

    [Fact]
    public async Task SendAsync_WithStoredToken_CallsNextHandler()
    {
        var (handler, innerHandler) = Given_we_have_a_handler_with_valid_token_and_inner_handler();
        var request = And_we_have_a_request();
        var response = await When_we_send_request(handler, request);
        Then_inner_handler_should_be_called(innerHandler, response);
    }

    [Fact]
    public async Task SendAsync_WithoutToken_CallsNextHandler()
    {
        var (handler, innerHandler) = Given_we_have_a_handler_with_no_token_and_inner_handler();
        var request = And_we_have_a_request();
        var response = await When_we_send_request(handler, request);
        Then_inner_handler_should_be_called(innerHandler, response);
    }

    [Fact]
    public async Task SendAsync_WithUnauthorizedResponse_ReturnsUnauthorizedResponse()
    {
        var handler = Given_we_have_a_handler_that_returns_unauthorized();
        var request = And_we_have_a_request();
        var response = await When_we_send_request(handler, request);
        Then_response_should_be_unauthorized(response);
    }

    #endregion

    #region Multiple Requests Tests

    [Fact]
    public async Task SendAsync_MultipleRequests_AttachesTokenToEachRequest()
    {
        var handler = Given_we_have_a_handler_with_valid_token();
        var (request1, request2) = And_we_have_multiple_requests();
        await When_we_send_multiple_requests(handler, request1, request2);
        Then_token_should_be_attached_to_both_requests(request1, request2);
    }

    #endregion

    #region Request Type Tests

    [Fact]
    public async Task SendAsync_WithPostRequest_AttachesToken()
    {
        var handler = Given_we_have_a_handler_with_valid_token();
        var request = And_we_have_a_post_request();
        await When_we_send_request(handler, request);
        Then_token_should_be_attached_to_request(request);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task SendAsync_WithCancellationToken_PassesCancellationTokenToInnerHandler()
    {
        var (handler, innerHandler) = Given_we_have_a_handler_with_valid_token_and_inner_handler();
        var request = And_we_have_a_request();
        var cancellationToken = And_we_have_a_cancellation_token();
        await When_we_send_request_with_cancellation_token(handler, request, cancellationToken);
        Then_cancellation_token_should_be_passed_to_inner_handler(innerHandler, cancellationToken);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SendAsync_WhenJSInteropThrows_ThrowsException()
    {
        var handler = Given_we_have_a_handler_that_throws_js_error();
        var request = And_we_have_a_request();
        await Then_sending_request_should_throw_exception(handler, request);
    }

    #endregion

    #region Given Methods

    JwtAuthorizationMessageHandler Given_we_have_a_handler_with_valid_token()
    {
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult(ValidToken);

        return new JwtAuthorizationMessageHandler(JSInterop.JSRuntime)
        {
            InnerHandler = new TestHttpMessageHandler()
        };
    }

    JwtAuthorizationMessageHandler Given_we_have_a_handler_with_no_token()
    {
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult((string)null!);

        return new JwtAuthorizationMessageHandler(JSInterop.JSRuntime)
        {
            InnerHandler = new TestHttpMessageHandler()
        };
    }

    JwtAuthorizationMessageHandler Given_we_have_a_handler_with_empty_token()
    {
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult(string.Empty);

        return new JwtAuthorizationMessageHandler(JSInterop.JSRuntime)
        {
            InnerHandler = new TestHttpMessageHandler()
        };
    }

    JwtAuthorizationMessageHandler Given_we_have_a_handler_with_whitespace_token()
    {
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult("   ");

        return new JwtAuthorizationMessageHandler(JSInterop.JSRuntime)
        {
            InnerHandler = new TestHttpMessageHandler()
        };
    }

    (JwtAuthorizationMessageHandler handler, TestHttpMessageHandler innerHandler) Given_we_have_a_handler_with_valid_token_and_inner_handler()
    {
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult(ValidToken);

        var innerHandler = new TestHttpMessageHandler();
        var handler = new JwtAuthorizationMessageHandler(JSInterop.JSRuntime)
        {
            InnerHandler = innerHandler
        };

        return (handler, innerHandler);
    }

    (JwtAuthorizationMessageHandler handler, TestHttpMessageHandler innerHandler) Given_we_have_a_handler_with_no_token_and_inner_handler()
    {
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult((string)null!);

        var innerHandler = new TestHttpMessageHandler();
        var handler = new JwtAuthorizationMessageHandler(JSInterop.JSRuntime)
        {
            InnerHandler = innerHandler
        };

        return (handler, innerHandler);
    }

    JwtAuthorizationMessageHandler Given_we_have_a_handler_that_returns_unauthorized()
    {
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetResult(ValidToken);

        var innerHandler = new TestHttpMessageHandler(HttpStatusCode.Unauthorized);
        return new JwtAuthorizationMessageHandler(JSInterop.JSRuntime)
        {
            InnerHandler = innerHandler
        };
    }

    JwtAuthorizationMessageHandler Given_we_have_a_handler_that_throws_js_error()
    {
        JSInterop.Setup<string>("localStorage.getItem", TokenKey)
            .SetException(new InvalidOperationException("JS runtime error"));

        return new JwtAuthorizationMessageHandler(JSInterop.JSRuntime)
        {
            InnerHandler = new TestHttpMessageHandler()
        };
    }

    #endregion

    #region And Methods

    HttpRequestMessage And_we_have_a_request()
    {
        return new HttpRequestMessage(HttpMethod.Get, "https://example.test/api/test");
    }

    HttpRequestMessage And_we_have_a_request_with_existing_auth_header()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/api/test");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "old-token");
        return request;
    }

    HttpRequestMessage And_we_have_a_post_request()
    {
        return new HttpRequestMessage(HttpMethod.Post, "https://example.test/api/test")
        {
            Content = new StringContent("{\"test\": \"data\"}")
        };
    }

    (HttpRequestMessage request1, HttpRequestMessage request2) And_we_have_multiple_requests()
    {
        var request1 = new HttpRequestMessage(HttpMethod.Get, "https://example.test/api/test1");
        var request2 = new HttpRequestMessage(HttpMethod.Get, "https://example.test/api/test2");
        return (request1, request2);
    }

    CancellationToken And_we_have_a_cancellation_token()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        return cancellationTokenSource.Token;
    }

    #endregion

    #region When Methods

    async Task<HttpResponseMessage> When_we_send_request(
        JwtAuthorizationMessageHandler handler,
        HttpRequestMessage request)
    {
        var invoker = new HttpMessageInvoker(handler);
        return await invoker.SendAsync(request, CancellationToken.None);
    }

    async Task<HttpResponseMessage> When_we_send_request_with_cancellation_token(
        JwtAuthorizationMessageHandler handler,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var invoker = new HttpMessageInvoker(handler);
        return await invoker.SendAsync(request, cancellationToken);
    }

    async Task When_we_send_multiple_requests(
        JwtAuthorizationMessageHandler handler,
        HttpRequestMessage request1,
        HttpRequestMessage request2)
    {
        var invoker = new HttpMessageInvoker(handler);
        await invoker.SendAsync(request1, CancellationToken.None);
        await invoker.SendAsync(request2, CancellationToken.None);
    }

    #endregion

    #region Then Methods

    void Then_token_should_be_attached_to_request(HttpRequestMessage request)
    {
        Assert.NotNull(request.Headers.Authorization);
        Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
        Assert.Equal(ValidToken, request.Headers.Authorization.Parameter);
    }

    void Then_token_should_not_be_attached(HttpRequestMessage request)
    {
        Assert.Null(request.Headers.Authorization);
    }

    void Then_token_should_be_overridden(HttpRequestMessage request)
    {
        Assert.NotNull(request.Headers.Authorization);
        Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
        Assert.Equal(ValidToken, request.Headers.Authorization.Parameter);
    }

    void Then_inner_handler_should_be_called(TestHttpMessageHandler innerHandler, HttpResponseMessage response)
    {
        Assert.True(innerHandler.SendAsyncCalled);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    void Then_response_should_be_unauthorized(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    void Then_token_should_be_attached_to_both_requests(HttpRequestMessage request1, HttpRequestMessage request2)
    {
        Assert.NotNull(request1.Headers.Authorization);
        Assert.Equal("Bearer", request1.Headers.Authorization.Scheme);
        Assert.Equal(ValidToken, request1.Headers.Authorization.Parameter);

        Assert.NotNull(request2.Headers.Authorization);
        Assert.Equal("Bearer", request2.Headers.Authorization.Scheme);
        Assert.Equal(ValidToken, request2.Headers.Authorization.Parameter);
    }

    void Then_cancellation_token_should_be_passed_to_inner_handler(
        TestHttpMessageHandler innerHandler,
        CancellationToken cancellationToken)
    {
        Assert.Equal(cancellationToken, innerHandler.CapturedCancellationToken);
    }

    async Task Then_sending_request_should_throw_exception(
        JwtAuthorizationMessageHandler handler,
        HttpRequestMessage request)
    {
        var invoker = new HttpMessageInvoker(handler);
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await invoker.SendAsync(request, CancellationToken.None));
    }

    #endregion

    #region Helper Classes

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        public bool SendAsyncCalled { get; private set; }
        public CancellationToken CapturedCancellationToken { get; private set; }

        public TestHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            SendAsyncCalled = true;
            CapturedCancellationToken = cancellationToken;
            return Task.FromResult(new HttpResponseMessage(_statusCode));
        }
    }

    #endregion
}
