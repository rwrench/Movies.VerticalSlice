namespace Movies.VerticalSlice.Api.Shared.Dtos;

public record MovieDto(
    Guid MovieId,
    string Title,
    string Slug,
    int YearOfRelease,
    string Genres,
    DateTime? DateUpdated
)
{
    public MovieDto() : this(default, string.Empty, string.Empty, default, string.Empty, DateTime.Now)
    {
    }
}
