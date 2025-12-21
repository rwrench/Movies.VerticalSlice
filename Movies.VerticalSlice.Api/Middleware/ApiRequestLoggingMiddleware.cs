using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Services;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Movies.VerticalSlice.Api.Middleware;

public class ApiRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiRequestLoggingMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ApiRequestLoggingMiddleware(
        RequestDelegate next, 
        ILogger<ApiRequestLoggingMiddleware> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for static files, health checks, and swagger
        if (ShouldSkipLogging(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Debug log to confirm middleware is running
        _logger.LogInformation("ApiRequestLoggingMiddleware: Processing {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            // Capture request details
            var requestBody = await ReadRequestBodyAsync(context.Request);
            var redactedRequestBody = RedactSensitiveData(requestBody, context.Request.Path);
            
            // Capture authenticated user info (from JWT token)
            var userId = context.User.Identity?.IsAuthenticated == true 
                ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                : null;
            var userName = context.User.Identity?.IsAuthenticated == true
                ? context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                : null;
            
            // For login requests, try to extract email from request body
            var isLoginRequest = context.Request.Path.Value?.Contains("/api/users/login") ?? false;
            if (isLoginRequest && string.IsNullOrEmpty(userName))
            {
                userName = ExtractEmailFromLoginRequest(requestBody);
            }
            
            // Debug: Log what user info we captured
            if (context.User.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation(
                    "ApiRequestLoggingMiddleware: Authenticated user - UserId: {UserId}, UserName: {UserName}, IsAuthenticated: {IsAuthenticated}", 
                    userId, userName, context.User.Identity.IsAuthenticated);
                
                // Log all claims for debugging
                var allClaims = string.Join(", ", context.User.Claims.Select(c => $"{c.Type}={c.Value}"));
                _logger.LogInformation("ApiRequestLoggingMiddleware: All claims: {Claims}", allClaims);
            }
            else
            {
                if (isLoginRequest && !string.IsNullOrEmpty(userName))
                {
                    _logger.LogInformation("ApiRequestLoggingMiddleware: Login attempt for email: {Email}", userName);
                }
                else
                {
                    _logger.LogInformation("ApiRequestLoggingMiddleware: User is NOT authenticated");
                }
            }
            
            var requestPath = context.Request.Path.Value;
            var method = context.Request.Method;
            var queryString = context.Request.QueryString.Value;
            
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Call the next middleware
            await _next(context);

            stopwatch.Stop();

            // Capture response details
            var responseBodyText = await ReadResponseBodyAsync(responseBody);
            var redactedResponseBody = RedactSensitiveData(responseBodyText, context.Request.Path);
            var statusCode = context.Response.StatusCode;
            var duration = stopwatch.ElapsedMilliseconds;
            
            // Copy response back to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);

            // Log to database asynchronously with a new scope
            _ = Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("ApiRequestLoggingMiddleware: Starting database log for {Method} {Path}", 
                        method, requestPath);
                    
                    using var scope = _serviceScopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<MoviesDbContext>();
                    
                    await LogRequestToDatabase(
                        method,
                        requestPath,
                        queryString,
                        userId,
                        userName,
                        redactedRequestBody,
                        redactedResponseBody,
                        statusCode,
                        duration,
                        dbContext);
                    
                    _logger.LogInformation("ApiRequestLoggingMiddleware: Successfully logged {Method} {Path} to database", 
                        method, requestPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to log API request to database for {Method} {Path}", method, requestPath);
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            var userId = context.User.Identity?.IsAuthenticated == true 
                ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                : null;
            var userName = context.User.Identity?.IsAuthenticated == true
                ? context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                : null;
            var requestPath = context.Request.Path.Value;
            var method = context.Request.Method;
            var queryString = context.Request.QueryString.Value;
            var duration = stopwatch.ElapsedMilliseconds;
            
            // Log error to database with a new scope
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<MoviesDbContext>();
                    
                    await LogErrorToDatabase(
                        method,
                        requestPath,
                        queryString,
                        userId,
                        userName,
                        ex,
                        duration,
                        dbContext);
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Failed to log error to database");
                }
            });
            
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private bool ShouldSkipLogging(PathString path)
    {
        var pathValue = path.Value?.ToLower() ?? string.Empty;
        
        return pathValue.Contains("/swagger") ||
               pathValue.Contains("/health") ||
               pathValue.Contains("/api/logs") ||
               pathValue.Contains("/_framework") ||
               pathValue.Contains("/_vs") ||
               pathValue.Contains(".css") ||
               pathValue.Contains(".js") ||
               pathValue.Contains(".map") ||
               pathValue.Contains(".ico") ||
               pathValue.Contains(".png") ||
               pathValue.Contains(".jpg") ||
               pathValue.Contains(".woff");
    }

    private bool ShouldSkipBodyLogging(PathString path)
    {
        var pathValue = path.Value?.ToLower() ?? string.Empty;
        
        // Skip body logging for authentication endpoints
        return pathValue.Contains("/api/users/register") ||
               pathValue.Contains("/api/users/login") ||
               pathValue.Contains("/api/auth");
    }

    private string? ExtractEmailFromLoginRequest(string? requestBody)
    {
        if (string.IsNullOrEmpty(requestBody))
            return null;

        try
        {
            var jsonDoc = JsonDocument.Parse(requestBody);
            var root = jsonDoc.RootElement;
            
            if (root.TryGetProperty("Email", out var emailElement) ||
                root.TryGetProperty("email", out emailElement))
            {
                return emailElement.GetString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to extract email from login request body");
        }

        return null;
    }

    private string? RedactSensitiveData(string? body, PathString path)
    {
        if (string.IsNullOrEmpty(body))
            return body;

        try
        {
            // For authentication endpoints, return a placeholder instead of actual data
            if (ShouldSkipBodyLogging(path))
                return "[REDACTED - Sensitive Authentication Data]";

            // For other endpoints, redact specific sensitive fields
            var jsonDoc = JsonDocument.Parse(body);
            var root = jsonDoc.RootElement;
            
            var redactedProperties = new Dictionary<string, object?>();
            foreach (var property in root.EnumerateObject())
            {
                var propertyName = property.Name.ToLower();
                
                // List of sensitive field names to redact
                if (propertyName is "password" or "token" or "accesstoken" or "refreshtoken" or 
                    "secret" or "apikey" or "authorization" or "creditcard" or "ssn")
                {
                    redactedProperties[property.Name] = "[REDACTED]";
                }
                else
                {
                    // Keep the original value for non-sensitive fields
                    redactedProperties[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
                }
            }
            
            return JsonSerializer.Serialize(redactedProperties);
        }
        catch
        {
            // If JSON parsing fails, return the original body
            // (it might not be JSON, or might be malformed)
            return body;
        }
    }

    private async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        if (request.ContentLength == null || request.ContentLength == 0)
            return null;

        request.EnableBuffering();

        try
        {
            using var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            // Truncate if too long
            return body.Length > 4000 ? body[..4000] + "..." : body;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to read request body");
            return null;
        }
    }

    private async Task<string?> ReadResponseBodyAsync(MemoryStream responseBody)
    {
        try
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseBody, leaveOpen: true);
            var text = await reader.ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            // Truncate if too long
            return text.Length > 4000 ? text[..4000] + "..." : text;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to read response body");
            return null;
        }
    }

    private async Task LogRequestToDatabase(
        string method,
        string requestPath,
        string queryString,
        string? userId,
        string? userName,
        string? requestBody,
        string? responseBody,
        int statusCode,
        long durationMs,
        MoviesDbContext dbContext)
    {
        var properties = new
        {
            Method = method,
            Path = requestPath,
            QueryString = queryString,
            StatusCode = statusCode,
            Duration = $"{durationMs}ms",
            UserId = userId,
            UserName = userName,
            RequestBody = requestBody,
            ResponseBody = responseBody,
            Headers = new Dictionary<string, string>()
        };

        var log = new ApplicationLog
        {
            Timestamp = DateTime.UtcNow,
            Level = statusCode >= 500 ? "Error" :
                    statusCode >= 400 ? "Warning" : "Information",
            Category = "ApiRequest",
            Message = $"{method} {requestPath} - {statusCode} ({durationMs}ms)" + 
                      (userName != null ? $" - User: {userName}" : ""),
            UserId = userId,
            UserName = userName,
            RequestPath = requestPath,
            Properties = JsonSerializer.Serialize(properties)
        };

        dbContext.ApplicationLogs.Add(log);
        await dbContext.SaveChangesAsync();
    }

    private async Task LogErrorToDatabase(
        string method,
        string requestPath,
        string queryString,
        string? userId,
        string? userName,
        Exception exception,
        long durationMs,
        MoviesDbContext dbContext)
    {
        var properties = new
        {
            Method = method,
            Path = requestPath,
            QueryString = queryString,
            Duration = $"{durationMs}ms",
            UserId = userId,
            UserName = userName
        };

        var log = new ApplicationLog
        {
            Timestamp = DateTime.UtcNow,
            Level = "Error",
            Category = "ApiRequest",
            Message = $"{method} {requestPath} - Exception: {exception.Message}" + 
                      (userName != null ? $" - User: {userName}" : ""),
            Exception = exception.ToString(),
            UserId = userId,
            UserName = userName,
            RequestPath = requestPath,
            Properties = JsonSerializer.Serialize(properties)
        };

        dbContext.ApplicationLogs.Add(log);
        await dbContext.SaveChangesAsync();
    }
}
