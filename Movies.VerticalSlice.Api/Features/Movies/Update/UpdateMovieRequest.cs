namespace Movies.VerticalSlice.Api.Features.Movies.Update;

public record UpdateMovieRequest(
string Title,
int YearOfRelease,
List<string> Genres,
Guid? UserId = null
);
