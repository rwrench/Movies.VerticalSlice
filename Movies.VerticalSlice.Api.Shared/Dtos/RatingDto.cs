namespace Movies.VerticalSlice.Api.Shared.Dtos;

public record RatingDto(
    Guid Id, 
    Guid MovieId, 
    float Rating, 
    string UserId, 
    DateTime DateLastUpdated);

