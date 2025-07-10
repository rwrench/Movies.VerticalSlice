using MediatR;

namespace Movies.VerticalSlice.Api.Features.Movies.Create;

public record CreateMovieCommand(
string Title,
int YearOfRelease,
List<string> Genres,
string? UserId = null
) : IRequest<Guid>;
