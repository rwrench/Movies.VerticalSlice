using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Features.LoginAudit.Shared;

namespace Movies.VerticalSlice.Api.Features.LoginAudit.GetAll;

public class GetLoginAuditLogsHandler : IRequestHandler<GetLoginAuditLogsQuery, GetLoginAuditLogsResponse>
{
    private readonly MoviesDbContext _context;
    private readonly ILogger<GetLoginAuditLogsHandler> _logger;

    public GetLoginAuditLogsHandler(MoviesDbContext context, ILogger<GetLoginAuditLogsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetLoginAuditLogsResponse> Handle(
        GetLoginAuditLogsQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate pagination parameters
            var page = Math.Max(query.Page, 1);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var logsQuery = _context.LoginAuditLogs.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(query.Email))
            {
                logsQuery = logsQuery.Where(l => l.Email.Contains(query.Email));
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                logsQuery = logsQuery.Where(l => l.Status == query.Status);
            }

            if (query.StartDate.HasValue)
            {
                logsQuery = logsQuery.Where(l => l.Timestamp >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                logsQuery = logsQuery.Where(l => l.Timestamp <= query.EndDate.Value);
            }

            // Get total count before pagination
            var totalCount = await logsQuery.CountAsync(cancellationToken);

            // Apply pagination
            var logs = await logsQuery
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
                "Retrieved login audit logs - Page: {Page}, PageSize: {PageSize}, Total: {Total}",
                page, pageSize, totalCount);

            return new GetLoginAuditLogsResponse(logs, totalCount, page, pageSize, totalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving login audit logs");
            throw;
        }
    }
}
