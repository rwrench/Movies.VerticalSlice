namespace Movies.VerticalSlice.Api.Features.Ratings.GetAll;


public record MovieRatingWithNameDto(
    Guid Id,
    Guid MovieId,
    float Rating,
    Guid UserId,
    DateTime DateUpdated,
    string MovieName);