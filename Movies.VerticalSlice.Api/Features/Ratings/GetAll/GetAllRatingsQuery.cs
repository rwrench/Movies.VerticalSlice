using MediatR;

namespace Movies.VerticalSlice.Api.Features.Ratings.GetAll;

public record GetAllRatingsQuery(): IRequest<IEnumerable<MovieRatingWithNameDto>>;

