using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Movies.VerticalSlice.Api.Middleware;

namespace Movies.Api.VerticalSlice.Api.Tests.Middleware;

public class ApiRequestLoggingMiddlewareTests
{
    [Fact]
    public void ShouldSkipLogging_SwaggerPath_ReturnsTrue()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var path = new PathString("/swagger/index.html");

        // Act
        var result = InvokeShouldSkipLogging(middleware, path);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkipLogging_ApiLogsPath_ReturnsTrue()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var path = new PathString("/api/logs");

        // Act
        var result = InvokeShouldSkipLogging(middleware, path);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkipLogging_RegularApiPath_ReturnsFalse()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var path = new PathString("/api/movies");

        // Act
        var result = InvokeShouldSkipLogging(middleware, path);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldSkipBodyLogging_LoginPath_ReturnsTrue()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var path = new PathString("/api/users/login");

        // Act
        var result = InvokeShouldSkipBodyLogging(middleware, path);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkipBodyLogging_RegisterPath_ReturnsTrue()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var path = new PathString("/api/users/register");

        // Act
        var result = InvokeShouldSkipBodyLogging(middleware, path);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RedactSensitiveData_PasswordField_IsRedacted()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var body = "{\"username\":\"test\",\"password\":\"secret123\"}";
        var path = new PathString("/api/test");

        // Act
        var result = InvokeRedactSensitiveData(middleware, body, path);

        // Assert
        result.Should().Contain("\"password\":\"[REDACTED]\"");
        result.Should().Contain("\"username\":\"test\"");
    }

    [Fact]
    public void RedactSensitiveData_AuthenticationEndpoint_ReturnsRedactedPlaceholder()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var body = "{\"username\":\"test\",\"password\":\"secret123\"}";
        var path = new PathString("/api/users/login");

        // Act
        var result = InvokeRedactSensitiveData(middleware, body, path);

        // Assert
        result.Should().Be("[REDACTED - Sensitive Authentication Data]");
    }

    [Fact]
    public void RedactSensitiveData_TokenField_IsRedacted()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var body = "{\"username\":\"test\",\"token\":\"abc123xyz\"}";
        var path = new PathString("/api/test");

        // Act
        var result = InvokeRedactSensitiveData(middleware, body, path);

        // Assert
        result.Should().Contain("\"token\":\"[REDACTED]\"");
    }

    [Fact]
    public void RedactSensitiveData_NonJsonBody_ReturnsOriginal()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var body = "This is not JSON";
        var path = new PathString("/api/test");

        // Act
        var result = InvokeRedactSensitiveData(middleware, body, path);

        // Assert
        result.Should().Be(body);
    }

    private ApiRequestLoggingMiddleware CreateMiddleware()
    {
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var logger = new Mock<ILogger<ApiRequestLoggingMiddleware>>();
        var serviceScopeFactory = new Mock<IServiceScopeFactory>();

        return new ApiRequestLoggingMiddleware(next, logger.Object, serviceScopeFactory.Object);
    }

    // Helper methods to invoke private methods via reflection
    private bool InvokeShouldSkipLogging(ApiRequestLoggingMiddleware middleware, PathString path)
    {
        var method = typeof(ApiRequestLoggingMiddleware)
            .GetMethod("ShouldSkipLogging", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (bool)method!.Invoke(middleware, new object[] { path })!;
    }

    private bool InvokeShouldSkipBodyLogging(ApiRequestLoggingMiddleware middleware, PathString path)
    {
        var method = typeof(ApiRequestLoggingMiddleware)
            .GetMethod("ShouldSkipBodyLogging", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (bool)method!.Invoke(middleware, new object[] { path })!;
    }

    private string? InvokeRedactSensitiveData(ApiRequestLoggingMiddleware middleware, string? body, PathString path)
    {
        var method = typeof(ApiRequestLoggingMiddleware)
            .GetMethod("RedactSensitiveData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (string?)method!.Invoke(middleware, new object?[] { body, path });
    }
}
