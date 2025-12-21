using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Features.LoginAudit.Shared;

namespace Movies.VerticalSlice.Api.Features.LoginAudit.GetByEmail;

public record GetLoginAuditLogsByEmailQuery(
    string Email,
    int Days = 7,
    int Page = 1,
    int PageSize = 50
) : IRequest<GetLoginAuditLogsByEmailResponse>;

public record GetLoginAuditLogsByEmailResponse(
    List<LoginAuditLogDto> Data,
    int TotalCount,
    int SuccessCount,
    int FailureCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

public class GetLoginAuditLogsByEmailHandler : IRequestHandler<GetLoginAuditLogsByEmailQuery, GetLoginAuditLogsByEmailResponse>
{
    private readonly MoviesDbContext _context;
    private readonly ILogger<GetLoginAuditLogsByEmailHandler> _logger;

    public GetLoginAuditLogsByEmailHandler(MoviesDbContext context, ILogger<GetLoginAuditLogsByEmailHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetLoginAuditLogsByEmailResponse> Handle(
        GetLoginAuditLogsByEmailQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var page = Math.Max(query.Page, 1);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);
            var cutoffDate = DateTime.UtcNow.AddDays(-query.Days);

            var logsQuery = _context.LoginAuditLogs
                .Where(l => l.Email == query.Email && l.Timestamp >= cutoffDate);

            var totalCount = await logsQuery.CountAsync(cancellationToken);
            var successCount = await logsQuery.Where(l => l.Status == "Success").CountAsync(cancellationToken);
            var failureCount = totalCount - successCount;

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
                "Retrieved login audit logs for email {Email} - Last {Days} days, Total: {Total}, Success: {Success}, Failure: {Failure}",
                query.Email, query.Days, totalCount, successCount, failureCount);

            return new GetLoginAuditLogsByEmailResponse(logs, totalCount, successCount, failureCount, page, pageSize, totalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving login audit logs for email: {Email}", query.Email);
            throw;
        }
    }
}
