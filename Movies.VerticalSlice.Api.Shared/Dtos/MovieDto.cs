using System.ComponentModel.DataAnnotations;

namespace Movies.VerticalSlice.Api.Shared.Dtos;

public class MovieDto
{
    public Guid MovieId { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Slug { get; set; } = string.Empty;
    [Required]
    [Range(1900, 2100)]
    public int YearOfRelease { get; set; }
    [Required]
    public string Genres { get; set; } = string.Empty;
    public DateTime? DateUpdated { get; set; }

    public MovieDto() { }

    public MovieDto(Guid movieId, string title, string slug, int yearOfRelease, string genres, DateTime? dateUpdated)
    {
        MovieId = movieId;
        Title = title;
        Slug = slug;
        YearOfRelease = yearOfRelease;
        Genres = genres;
        DateUpdated = dateUpdated;
    }
}
