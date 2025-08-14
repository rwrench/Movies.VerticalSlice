namespace Movies.VerticalSlice.Api.Shared.Dtos;

public class MovieRatingWithNameDto
{
    public MovieRatingWithNameDto() { }
    public MovieRatingWithNameDto(
        Guid id, 
        Guid movieId, 
        float rating, 
        string userId,
        DateTime? dateUpdated, 
        string movieName, 
        string genres,
        string userName)
    {
        Id = id;
        MovieId = movieId;
        Rating = rating;
        UserId = userId;
        DateUpdated = dateUpdated;
        MovieName = movieName;
        Genres = genres;
        UserName = userName;
    }

    public Guid Id { get; set; } = Guid.Empty;
    public Guid MovieId { get; set; } = Guid.Empty;
    public float Rating { get; set; } = 0.5f;
    public string? UserId { get; set; } = string.Empty;
    public DateTime? DateUpdated { get; set; } = null;  
    public string MovieName { get; set; } = string.Empty;
    public string Genres { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}