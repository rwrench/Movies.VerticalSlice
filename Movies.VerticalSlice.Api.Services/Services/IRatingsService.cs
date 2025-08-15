using Movies.VerticalSlice.Api.Shared.Dtos;

namespace Movies.VerticalSlice.Api.Services
{
    public interface IRatingsService
    {
        string? AuthToken { get; set; }

        Task<HttpResponseMessage> CreateAsync(MovieRatingWithNameDto rating);
        Task<HttpResponseMessage> DeleteAsync(Guid id);
        Task<List<MovieRatingWithNameDto>?> GetAllAsync();
        Task<List<MovieNameDto>?> GetAllMovieNamesAsync(string title = "");
        Task<HttpResponseMessage> UpdateAsync(MovieRatingWithNameDto rating);
    }
}