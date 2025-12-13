namespace Movies.VerticalSlice.Api.Data.Models;

public class ApplicationLog
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public required string Level { get; set; }
    public required string Category { get; set; }
    public required string Message { get; set; }
    public string? Exception { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? RequestPath { get; set; }
    public string? Properties { get; set; }
}
