using MediatR;

namespace Movies.VerticalSlice.Api.Features.Ratings.GetAll;

public record GetAllRatingsQuery(
    Guid id
    ): IRequest<IEnumerable<MovieRatingWithNameDto>>;

