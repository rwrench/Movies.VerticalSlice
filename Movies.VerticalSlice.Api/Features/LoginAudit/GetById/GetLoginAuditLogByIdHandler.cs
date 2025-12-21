using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Features.LoginAudit.Shared;

namespace Movies.VerticalSlice.Api.Features.LoginAudit.GetById;

public record GetLoginAuditLogByIdQuery(long Id) : IRequest<LoginAuditLogDetailDto?>;

public class GetLoginAuditLogByIdHandler : IRequestHandler<GetLoginAuditLogByIdQuery, LoginAuditLogDetailDto?>
{
    private readonly MoviesDbContext _context;
    private readonly ILogger<GetLoginAuditLogByIdHandler> _logger;

    public GetLoginAuditLogByIdHandler(MoviesDbContext context, ILogger<GetLoginAuditLogByIdHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LoginAuditLogDetailDto?> Handle(
        GetLoginAuditLogByIdQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var log = await _context.LoginAuditLogs
                .Where(l => l.Id == query.Id)
                .Select(l => new LoginAuditLogDetailDto(
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
                .FirstOrDefaultAsync(cancellationToken);

            return log;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving login audit log with ID: {Id}", query.Id);
            throw;
        }
    }
}
