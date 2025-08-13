using MediatR;
using Movies.VerticalSlice.Api.Shared.Dtos;


namespace Movies.VerticalSlice.Api.Features.Movies.GetAll;

public record GetAllMoviesQuery(
     string? Title = null,
     int? YearOfRelease = null,
     string? SortField = null,
     SortOrder? SortOrder = null,
     int? Limit = null
) : IRequest<IEnumerable<MovieDto>>;
