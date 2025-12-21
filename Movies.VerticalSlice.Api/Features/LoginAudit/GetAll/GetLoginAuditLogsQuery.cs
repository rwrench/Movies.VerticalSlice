using MediatR;
using Movies.VerticalSlice.Api.Features.LoginAudit.Shared;

namespace Movies.VerticalSlice.Api.Features.LoginAudit.GetAll;

public record GetLoginAuditLogsQuery(
    int Page = 1,
    int PageSize = 50,
    string? Email = null,
    string? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<GetLoginAuditLogsResponse>;

public record GetLoginAuditLogsResponse(
    List<LoginAuditLogDto> Data,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
