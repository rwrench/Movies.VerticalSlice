namespace Movies.VerticalSlice.Api.Features.Movies.Update;

public record UpdateMovieRequest(
string Title,
int YearOfRelease,
string Genres,
Guid? UserId = null
);
