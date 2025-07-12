using MediatR;

namespace Movies.VerticalSlice.Api.Features.Ratings.Create;

public record RateMovieCommand(
    Guid MovieId, 
    float Rating, 
    DateTime DateUpdated,
    string UserId) : IRequest<bool>;


