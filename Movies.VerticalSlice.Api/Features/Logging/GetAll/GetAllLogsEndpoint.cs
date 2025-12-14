using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;

namespace Movies.VerticalSlice.Api.Features.Logging.GetAll;

public static class GetAllLogsEndpoint
{
    public static void MapGetAllLogs(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/logs", async (
            MoviesDbContext db,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? level = null,
            [FromQuery] string? category = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null) =>
        {
            var query = db.ApplicationLogs.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(l => l.Level == level);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(l => l.Category == category);
            }

            if (startDate.HasValue)
            {
                query = query.Where(l => l.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(l => l.Timestamp <= endDate.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new
                {
                    l.Id,
                    l.Timestamp,
                    l.Level,
                    l.Category,
                    l.Message,
                    l.Exception,
                    l.UserId,
                    l.UserName,
                    l.RequestPath,
                    l.Properties
                })
                .ToListAsync();

            return Results.Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Logs = logs
            });
        })
        .WithName("GetAllLogs")
        .WithTags("Logging")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<object>(StatusCodes.Status200OK);
    }
}
