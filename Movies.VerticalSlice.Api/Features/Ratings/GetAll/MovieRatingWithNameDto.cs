namespace Movies.VerticalSlice.Api.Features.Ratings.GetAll;


public record MovieRatingWithNameDto(
    Guid Id,
    Guid MovieId,
    float Rating,
    string UserId,
    DateTime? DateUpdated,
    string MovieName,
    string Genres);