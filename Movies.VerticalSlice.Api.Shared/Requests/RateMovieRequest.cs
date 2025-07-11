namespace Movies.VerticalSlice.Api.Shared.Requests;

public record RateMovieRequest(
    float Rating,
    Guid MovieId,
    string UserId, 
    DateTime DateUpdated);
