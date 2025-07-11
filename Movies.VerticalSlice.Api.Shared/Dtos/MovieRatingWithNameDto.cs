namespace Movies.VerticalSlice.Api.Shared.Dtos;


public record MovieRatingWithNameDto(
    Guid Id,
    Guid MovieId,
    float Rating,
    string UserId,
    DateTime? DateUpdated,
    string MovieName,
    string Genres);