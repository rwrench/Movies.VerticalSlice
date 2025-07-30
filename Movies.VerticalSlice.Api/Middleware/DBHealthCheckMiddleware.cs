using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;

namespace Movies.VerticalSlice.Api.Middleware
{
    public class DbHealthCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public DbHealthCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, HealthCheckService healthCheckService)
        {
            // Allow /health requests to pass through to the endpoint
            if (context.Request.Path.StartsWithSegments("/health"))
            {
                await _next(context);
                return;
            }

            // For all other requests, check DB health
            var report = await healthCheckService.CheckHealthAsync();
            if (report.Status != HealthStatus.Healthy)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("""
                {
                    "error": "The database is unavailable. Please try again later."
                }
                """);
                return;
            }

            await _next(context);
        }
    }
}
