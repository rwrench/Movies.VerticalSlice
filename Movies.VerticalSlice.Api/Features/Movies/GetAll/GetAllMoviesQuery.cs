using MediatR;
using Microsoft.Data.SqlClient;

namespace Movies.VerticalSlice.Api.Features.Movies.GetAll;

public record GetAllMoviesQuery(
 string? Title = null,
 int? YearOfRelease = null,
 string? SortField = null,
 SortOrder? SortOrder = null,
 int? Page = null,
 int? PageSize = null
) : IRequest<List<MovieDto>>;

public record MovieDto(
    Guid MovieId,
    string Title,
    string Slug,
    int YearOfRelease,
    List<string> Genres
);

public enum SortOrder
{
    Unsorted,
    Ascending,
    Descending
}