using MediatR;

namespace Movies.VerticalSlice.Api.Features.Movies.Create;

public record CreateMovieCommand(
string Title,
int YearOfRelease,
List<string> Genres,
Guid? UserId = null
) : IRequest<Guid>;
