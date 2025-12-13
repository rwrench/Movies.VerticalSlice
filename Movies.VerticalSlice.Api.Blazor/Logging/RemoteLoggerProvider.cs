using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Movies.VerticalSlice.Api.Blazor.Logging;

public class RemoteLoggerProvider : ILoggerProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LogLevel _minimumLevel;

    public RemoteLoggerProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        
        // Read minimum level from configuration, default to Information
        var configValue = configuration["Logging:RemoteLogger:MinimumLevel"];
        _minimumLevel = Enum.TryParse<LogLevel>(configValue, out var level) 
            ? level 
            : LogLevel.Information;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new RemoteLogger(_httpClientFactory, categoryName, _minimumLevel);
    }

    public void Dispose() { }
}

public class RemoteLogger : ILogger
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _categoryName;
    private readonly LogLevel _minimumLevel;

    public RemoteLogger(IHttpClientFactory httpClientFactory, string categoryName, LogLevel minimumLevel)
    {
        _httpClientFactory = httpClientFactory;
        _categoryName = categoryName;
        _minimumLevel = minimumLevel;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        // Get the formatted message from the formatter
        var message = formatter(state, exception);
        
        // Extract structured properties if available
        string? propertiesJson = null;
        if (state is IEnumerable<KeyValuePair<string, object>> properties)
        {
            try
            {
                var dict = properties.ToDictionary(p => p.Key, p => p.Value);
                propertiesJson = JsonSerializer.Serialize(dict);
            }
            catch
            {
                // If serialization fails, just skip properties
            }
        }

        var logRequest = new
        {
            Level = logLevel.ToString(),
            Category = _categoryName,
            Message = message,
            Exception = exception?.ToString(),
            RequestPath = null as string,
            Properties = propertiesJson
        };

        // Fire and forget - don't wait for logging to complete
        _ = SendLogAsync(logRequest);
    }

    private async Task SendLogAsync(object logRequest)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("LoggingClient");
            
            // Set a short timeout to prevent hanging
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await client.PostAsJsonAsync("/api/logs", logRequest, cts.Token);
        }
        catch
        {
            // Silently fail - logging should never break the app
        }
    }
}
