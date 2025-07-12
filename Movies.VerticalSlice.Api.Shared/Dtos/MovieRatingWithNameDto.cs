namespace Movies.VerticalSlice.Api.Shared.Dtos;


public record MovieRatingWithNameDto(
    Guid Id,
    Guid MovieId,
    float Rating,
    string? UserId,
    DateTime? DateUpdated,
    string MovieName,
    string Genres)
{
    public MovieRatingWithNameDto() : this(default, default, 0, default, default, string.Empty, string.Empty)
    {
    }
}