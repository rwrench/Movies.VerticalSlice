namespace Movies.VerticalSlice.Api.Data.Models;

public class MovieRating
{
    public required Guid Id { get; set; }
    public required Guid MovieId { get; set; }
    public required float Rating { get; set; }
    public Guid UserId { get; set; }
    public DateTime? DateUpdated { get; set; } = DateTime.UtcNow;
}