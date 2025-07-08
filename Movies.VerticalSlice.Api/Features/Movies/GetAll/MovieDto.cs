namespace Movies.VerticalSlice.Api.Features.Movies.GetAll;

public record MovieDto(
    Guid MovieId,
    string Title,
    string Slug,
    int YearOfRelease,
    List<string> Genres
);
