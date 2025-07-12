using MediatR;
using Movies.VerticalSlice.Api.Shared.Dtos;

namespace Movies.VerticalSlice.Api.Features.Ratings.GetAll;

public record GetAllRatingsQuery(): IRequest<IEnumerable<MovieRatingWithNameDto>>;

