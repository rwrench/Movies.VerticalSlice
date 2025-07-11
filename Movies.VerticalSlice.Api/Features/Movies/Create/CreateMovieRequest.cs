namespace Movies.VerticalSlice.Api.Features.Movies.Create
{
    public record CreateMovieRequest(
        string Title,
        int YearOfRelease,
        string Genres,
        Guid? UserId = null
    );

}
