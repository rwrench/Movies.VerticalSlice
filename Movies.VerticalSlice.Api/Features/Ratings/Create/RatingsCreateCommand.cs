using MediatR;

namespace Movies.VerticalSlice.Api.Features.Ratings.Create;

public record RatingsCreateCommand(
    Guid MovieId, 
    float Rating, 
    DateTime DateUpdated,
    string UserId) : IRequest<Guid>;


