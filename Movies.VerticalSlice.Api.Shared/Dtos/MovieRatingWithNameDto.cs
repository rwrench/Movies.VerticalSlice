namespace Movies.VerticalSlice.Api.Shared.Dtos;

public class MovieRatingWithNameDto
{
    public MovieRatingWithNameDto(
        Guid id, 
        Guid movieId, 
        float rating, 
        string userId,
        DateTime? dateUpdated, 
        string movieName, 
        string genres)
    {
        Id = id;
        MovieId = movieId;
        Rating = rating;
        UserId = userId;
        DateUpdated = dateUpdated;
        MovieName = movieName;
        Genres = genres;
    }

    public Guid Id { get; set; }
    public Guid MovieId { get; set; } 
    public float Rating { get; set; }
    public string? UserId { get; set; }
    public DateTime? DateUpdated { get; set; }
    public string MovieName { get; set; } = string.Empty;
    public string Genres { get; set; } = string.Empty;
   
}