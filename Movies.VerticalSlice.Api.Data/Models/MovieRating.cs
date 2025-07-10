namespace Movies.VerticalSlice.Api.Data.Models;

public class MovieRating
{
    public required Guid Id { get; set; }
    public required float Rating { get; set; }
    public DateTime? DateUpdated { get; set; } = DateTime.UtcNow;

    public Guid MovieId { get; set; }
    public Movie Movie { get; set; } = null!;

    public required string UserId { get; set; }
}