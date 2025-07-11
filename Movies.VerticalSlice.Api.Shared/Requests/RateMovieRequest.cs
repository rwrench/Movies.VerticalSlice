namespace Movies.VerticalSlice.Api.Shared.Requests;

public record RateMovieRequest(
    Guid MovieId,
    float Rating,
    DateTime? DateUpdated,
    string? UserId = null);
