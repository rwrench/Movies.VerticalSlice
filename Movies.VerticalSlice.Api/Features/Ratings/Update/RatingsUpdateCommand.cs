using MediatR;

namespace Movies.VerticalSlice.Api.Features.Ratings.Update;

public record RatingsUpdateCommand(
    Guid RatingsId,
    Guid MovieId,   
    float Rating,
    DateTime DateUpdated,
    string UserId) : IRequest<bool>;
