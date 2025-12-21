using Microsoft.AspNetCore.Mvc;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Services;

namespace Movies.VerticalSlice.Api.Features.Logging.Create;

public static class CreateLogEndpoint
{
    public static void MapCreateLog(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/logs", async (
            [FromBody] CreateLogRequest request,
            MoviesDbContext db,
            UserContextService userContextService,
            HttpContext httpContext,
            ILogger<CreateLogRequest> logger) =>
        {
            try
            {
                // Debug: Log what we received
                logger.LogInformation("Received log request - Level: {Level}, Category: {Category}, Message: {Message}",
                    request.Level, request.Category, request.Message);

                // Capture user info if authenticated
                string? userId = null;
                string? userName = null;
                
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    userId = userContextService.GetCurrentUserId();
                    userName = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                }

                var log = new ApplicationLog
                {
                    Timestamp = DateTime.UtcNow,
                    Level = request.Level ?? "Information",
                    Category = request.Category ?? "Unknown",
                    Message = request.Message ?? "No message",
                    Exception = request.Exception,
                    UserId = userId,
                    UserName = userName,
                    RequestPath = request.RequestPath,
                    Properties = request.Properties
                };

                db.ApplicationLogs.Add(log);
                await db.SaveChangesAsync();

                return Results.Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving application log");
                return Results.StatusCode(500);
            }
        })
        .WithName("CreateLog")
        .WithTags("Logging")
        .WithOpenApi()
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}

public record CreateLogRequest(
    string? Level,
    string? Category,
    string? Message,
    string? Exception,
    string? RequestPath,
    string? Properties
);
