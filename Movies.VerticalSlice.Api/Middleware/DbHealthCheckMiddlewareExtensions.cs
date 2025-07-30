using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Movies.VerticalSlice.Api.Middleware;

public static class DbHealthCheckMiddlewareExtensions
{
    public static IApplicationBuilder UseDbHealthCheck(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HealthCheckMiddleware>();
    }
}
