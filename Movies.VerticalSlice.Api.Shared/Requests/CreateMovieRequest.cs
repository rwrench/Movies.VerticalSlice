namespace Movies.VerticalSlice.Api.Shared.Requests;

public record CreateMovieRequest(
    string Title,
    int YearOfRelease,
    string Genres,
    Guid? UserId = null
);


