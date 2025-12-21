namespace Movies.VerticalSlice.Api.Features.LoginAudit.Shared;

public record LoginAuditLogDto(
    long Id,
    DateTime Timestamp,
    string Email,
    string Status,
    string? UserId,
    string? UserName,
    string? FailureReason,
    string? IpAddress,
    string? UserAgent
);

public record LoginAuditLogDetailDto(
    long Id,
    DateTime Timestamp,
    string Email,
    string Status,
    string? UserId,
    string? UserName,
    string? FailureReason,
    string? IpAddress,
    string? UserAgent
);
