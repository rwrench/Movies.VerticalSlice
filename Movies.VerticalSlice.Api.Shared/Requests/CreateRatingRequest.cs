namespace Movies.VerticalSlice.Api.Shared.Requests;

public record CreateRatingRequest(
    Guid MovieId,
    float Rating,
    DateTime? DateUpdated,
    string? UserId = null);
