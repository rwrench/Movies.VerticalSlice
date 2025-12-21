namespace Movies.VerticalSlice.Api.Data.Models;

/// <summary>
/// Audit log for login attempts (both successful and failed).
/// Used for security monitoring and compliance.
/// </summary>
public class LoginAuditLog
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public required string Email { get; set; }
    public required string Status { get; set; } // "Success", "InvalidCredentials", "UserNotFound", "ValidationError"
    public string? UserId { get; set; } // Only populated for successful logins
    public string? UserName { get; set; } // Only populated for successful logins
    public string? FailureReason { get; set; } // Details about why login failed
    public string? IpAddress { get; set; } // IP address of the login attempt
    public string? UserAgent { get; set; } // Browser/client info
}
