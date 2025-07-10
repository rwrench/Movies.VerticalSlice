namespace Movies.VerticalSlice.Api.Features.Ratings.GetAll;

public record RatingDto(
    Guid Id, 
    Guid MovieId, 
    float Rating, 
    string UserId, 
    DateTime DateLastUpdated);

