using MediatR;

namespace Movies.VerticalSlice.Api.Features.Ratings.Delete;

public record DeleteRatingCommand(Guid RatingId): IRequest<bool>;
