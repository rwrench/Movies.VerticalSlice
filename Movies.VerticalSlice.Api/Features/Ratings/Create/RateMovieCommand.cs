using MediatR;

namespace Movies.VerticalSlice.Api.Features.Ratings.Create;

public record RateMovieCommand(
    Guid MovieId, 
    float Rating, 
    string UserId, 
    DateTime DateUpdated) : IRequest<bool>;


