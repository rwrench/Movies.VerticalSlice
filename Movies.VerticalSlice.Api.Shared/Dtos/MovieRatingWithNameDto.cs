namespace Movies.VerticalSlice.Api.Shared.Dtos;

public class MovieRatingWithNameDto
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; } // <-- must be settable!
    public float Rating { get; set; }
    public string? UserId { get; set; }
    public DateTime? DateUpdated { get; set; }
    public string MovieName { get; set; } = string.Empty;
    public string Genres { get; set; } = string.Empty;
}