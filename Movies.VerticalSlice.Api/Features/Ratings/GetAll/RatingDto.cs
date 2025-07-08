namespace Movies.VerticalSlice.Api.Features.Ratings.GetAll;

public record RatingDto(
    Guid Id, 
    Guid MovieId, 
    float Rating, 
    Guid UserId, 
    DateTime DateLastUpdated);

