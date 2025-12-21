using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Data.Models;

namespace Movies.VerticalSlice.Api.Services;

/// <summary>
/// Service for logging login attempts (both successful and failed).
/// Provides security audit trail for compliance and monitoring.
/// </summary>
public interface ILoginAuditService
{
    /// <summary>
    /// Log a successful login attempt.
    /// </summary>
    Task LogSuccessfulLoginAsync(
        string email,
        string userId,
        string userName,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log a failed login attempt.
    /// </summary>
    Task LogFailedLoginAsync(
        string email,
        string failureReason,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
}

public class LoginAuditService : ILoginAuditService
{
    private readonly MoviesDbContext _context;
    private readonly ILogger<LoginAuditService> _logger;

    public LoginAuditService(MoviesDbContext context, ILogger<LoginAuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogSuccessfulLoginAsync(
        string email,
        string userId,
        string userName,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new LoginAuditLog
            {
                Timestamp = DateTime.UtcNow,
                Email = email,
                Status = "Success",
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.LoginAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Login audit logged - Email: {Email}, Status: {Status}, UserId: {UserId}",
                email, "Success", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log successful login attempt for email: {Email}", email);
            // Don't throw - logging failure shouldn't break the login process
        }
    }

    public async Task LogFailedLoginAsync(
        string email,
        string failureReason,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new LoginAuditLog
            {
                Timestamp = DateTime.UtcNow,
                Email = email,
                Status = failureReason switch
                {
                    "User not found" => "UserNotFound",
                    "Invalid password" => "InvalidCredentials",
                    _ => "FailedLogin"
                },
                FailureReason = failureReason,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.LoginAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Login audit logged - Email: {Email}, Status: Failed, Reason: {Reason}",
                email, failureReason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log failed login attempt for email: {Email}", email);
            // Don't throw - logging failure shouldn't break the login process
        }
    }
}
