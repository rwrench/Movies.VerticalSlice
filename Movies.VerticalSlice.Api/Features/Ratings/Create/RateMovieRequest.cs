namespace Movies.VerticalSlice.Api.Features.Ratings.Create;

public record RateMovieRequest(
    float Rating,
    Guid MovieId,
    string UserId, 
    DateTime DateUpdated);
