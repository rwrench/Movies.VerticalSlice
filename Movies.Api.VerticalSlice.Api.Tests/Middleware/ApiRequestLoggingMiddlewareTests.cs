using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Movies.VerticalSlice.Api.Middleware;

namespace Movies.Api.VerticalSlice.Api.Tests.Middleware;

public class ApiRequestLoggingMiddlewareTests
{
    #region ShouldSkipLogging Tests

    [Fact]
    public void ShouldSkipLogging_SwaggerPath_ReturnsTrue()
    {
        var middleware = Given_we_have_middleware();
        var path = And_we_have_swagger_path();
        var result = When_we_check_should_skip_logging(middleware, path);
        Then_result_should_be_true(result);
    }

    [Fact]
    public void ShouldSkipLogging_ApiLogsPath_ReturnsTrue()
    {
        var middleware = Given_we_have_middleware();
        var path = And_we_have_api_logs_path();
        var result = When_we_check_should_skip_logging(middleware, path);
        Then_result_should_be_true(result);
    }

    [Fact]
    public void ShouldSkipLogging_RegularApiPath_ReturnsFalse()
    {
        var middleware = Given_we_have_middleware();
        var path = And_we_have_regular_api_path();
        var result = When_we_check_should_skip_logging(middleware, path);
        Then_result_should_be_false(result);
    }

    #endregion

    #region ShouldSkipBodyLogging Tests

    [Fact]
    public void ShouldSkipBodyLogging_LoginPath_ReturnsTrue()
    {
        var middleware = Given_we_have_middleware();
        var path = And_we_have_login_path();
        var result = When_we_check_should_skip_body_logging(middleware, path);
        Then_result_should_be_true(result);
    }

    [Fact]
    public void ShouldSkipBodyLogging_RegisterPath_ReturnsTrue()
    {
        var middleware = Given_we_have_middleware();
        var path = And_we_have_register_path();
        var result = When_we_check_should_skip_body_logging(middleware, path);
        Then_result_should_be_true(result);
    }

    #endregion

    #region RedactSensitiveData Tests

    [Fact]
    public void RedactSensitiveData_PasswordField_IsRedacted()
    {
        var middleware = Given_we_have_middleware();
        var body = And_we_have_body_with_password();
        var path = And_we_have_test_path();
        var result = When_we_redact_sensitive_data(middleware, body, path);
        Then_password_should_be_redacted(result);
        And_username_should_remain_visible(result);
    }

    [Fact]
    public void RedactSensitiveData_AuthenticationEndpoint_ReturnsRedactedPlaceholder()
    {
        var middleware = Given_we_have_middleware();
        var body = And_we_have_authentication_body();
        var path = And_we_have_login_path();
        var result = When_we_redact_sensitive_data(middleware, body, path);
        Then_result_should_be_authentication_placeholder(result);
    }

    [Fact]
    public void RedactSensitiveData_TokenField_IsRedacted()
    {
        var middleware = Given_we_have_middleware();
        var body = And_we_have_body_with_token();
        var path = And_we_have_test_path();
        var result = When_we_redact_sensitive_data(middleware, body, path);
        Then_token_should_be_redacted(result);
    }

    [Fact]
    public void RedactSensitiveData_NonJsonBody_ReturnsOriginal()
    {
        var middleware = Given_we_have_middleware();
        var body = And_we_have_non_json_body();
        var path = And_we_have_test_path();
        var result = When_we_redact_sensitive_data(middleware, body, path);
        Then_result_should_be_original(result, body);
    }

    #endregion

    #region Given Methods

    ApiRequestLoggingMiddleware Given_we_have_middleware()
    {
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var logger = new Mock<ILogger<ApiRequestLoggingMiddleware>>();
        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        return new ApiRequestLoggingMiddleware(next, logger.Object, serviceScopeFactory.Object);
    }

    #endregion

    #region And Methods

    PathString And_we_have_swagger_path()
    {
        return new PathString("/swagger/index.html");
    }

    PathString And_we_have_api_logs_path()
    {
        return new PathString("/api/logs");
    }

    PathString And_we_have_regular_api_path()
    {
        return new PathString("/api/movies");
    }

    PathString And_we_have_login_path()
    {
        return new PathString("/api/users/login");
    }

    PathString And_we_have_register_path()
    {
        return new PathString("/api/users/register");
    }

    PathString And_we_have_test_path()
    {
        return new PathString("/api/test");
    }

    string And_we_have_body_with_password()
    {
        return "{\"username\":\"test\",\"password\":\"secret123\"}";
    }

    string And_we_have_authentication_body()
    {
        return "{\"username\":\"test\",\"password\":\"secret123\"}";
    }

    string And_we_have_body_with_token()
    {
        return "{\"username\":\"test\",\"token\":\"abc123xyz\"}";
    }

    string And_we_have_non_json_body()
    {
        return "This is not JSON";
    }

    void And_username_should_remain_visible(string result)
    {
        result.Should().Contain("\"username\":\"test\"");
    }

    #endregion

    #region When Methods

    bool When_we_check_should_skip_logging(ApiRequestLoggingMiddleware middleware, PathString path)
    {
        var method = typeof(ApiRequestLoggingMiddleware)
            .GetMethod("ShouldSkipLogging", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (bool)method!.Invoke(middleware, new object[] { path })!;
    }

    bool When_we_check_should_skip_body_logging(ApiRequestLoggingMiddleware middleware, PathString path)
    {
        var method = typeof(ApiRequestLoggingMiddleware)
            .GetMethod("ShouldSkipBodyLogging", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (bool)method!.Invoke(middleware, new object[] { path })!;
    }

    string? When_we_redact_sensitive_data(ApiRequestLoggingMiddleware middleware, string? body, PathString path)
    {
        var method = typeof(ApiRequestLoggingMiddleware)
            .GetMethod("RedactSensitiveData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (string?)method!.Invoke(middleware, new object?[] { body, path });
    }

    #endregion

    #region Then Methods

    void Then_result_should_be_true(bool result)
    {
        result.Should().BeTrue();
    }

    void Then_result_should_be_false(bool result)
    {
        result.Should().BeFalse();
    }

    void Then_password_should_be_redacted(string result)
    {
        result.Should().Contain("\"password\":\"[REDACTED]\"");
    }

    void Then_result_should_be_authentication_placeholder(string result)
    {
        result.Should().Be("[REDACTED - Sensitive Authentication Data]");
    }

    void Then_token_should_be_redacted(string result)
    {
        result.Should().Contain("\"token\":\"[REDACTED]\"");
    }

    void Then_result_should_be_original(string result, string expected)
    {
        result.Should().Be(expected);
    }

    #endregion
}
