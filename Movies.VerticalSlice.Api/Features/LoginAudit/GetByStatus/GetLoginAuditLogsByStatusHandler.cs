using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Features.LoginAudit.Shared;

namespace Movies.VerticalSlice.Api.Features.LoginAudit.GetByStatus;

public record GetLoginAuditLogsByStatusQuery(
    string Status, // "Success", "UserNotFound", "InvalidCredentials", "FailedLogin"
    int Days = 7,
    int Page = 1,
    int PageSize = 50
) : IRequest<GetLoginAuditLogsByStatusResponse>;

public record GetLoginAuditLogsByStatusResponse(
    List<LoginAuditLogDto> Data,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages,
    string Status,
    Dictionary<string, int> StatusBreakdown
);

public class GetLoginAuditLogsByStatusHandler : IRequestHandler<GetLoginAuditLogsByStatusQuery, GetLoginAuditLogsByStatusResponse>
{
    private readonly MoviesDbContext _context;
    private readonly ILogger<GetLoginAuditLogsByStatusHandler> _logger;

    public GetLoginAuditLogsByStatusHandler(MoviesDbContext context, ILogger<GetLoginAuditLogsByStatusHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetLoginAuditLogsByStatusResponse> Handle(
        GetLoginAuditLogsByStatusQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var page = Math.Max(query.Page, 1);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);
            var cutoffDate = DateTime.UtcNow.AddDays(-query.Days);

            var logsQuery = _context.LoginAuditLogs
                .Where(l => l.Timestamp >= cutoffDate);

            // Get status breakdown
            var statusBreakdown = await logsQuery
                .GroupBy(l => l.Status)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

            // Filter by status
            var filteredLogsQuery = logsQuery.Where(l => l.Status == query.Status);
            var totalCount = await filteredLogsQuery.CountAsync(cancellationToken);

            var logs = await filteredLogsQuery
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LoginAuditLogDto(
                    l.Id,
                    l.Timestamp,
                    l.Email,
                    l.Status,
                    l.UserId,
                    l.UserName,
                    l.FailureReason,
                    l.IpAddress,
                    l.UserAgent
                ))
                .ToListAsync(cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            _logger.LogInformation(
                "Retrieved login audit logs by status {Status} - Last {Days} days, Total: {Total}",
                query.Status, query.Days, totalCount);

            return new GetLoginAuditLogsByStatusResponse(
                logs, 
                totalCount, 
                page, 
                pageSize, 
                totalPages, 
                query.Status, 
                statusBreakdown);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving login audit logs by status: {Status}", query.Status);
            throw;
        }
    }
}
