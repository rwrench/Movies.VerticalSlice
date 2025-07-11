namespace Movies.VerticalSlice.Api.Shared.Requests;

public record UpdateMovieRequest(
string Title,
int YearOfRelease,
string Genres,
string? UserId = null
);
