using Movies.VerticalSlice.Api.Shared.Dtos;

namespace Movies.VerticalSlice.Api.Services
{
    public interface IMovieService
    {
        Task<HttpResponseMessage> CreateAsync(MovieDto movie);
        Task<HttpResponseMessage> DeleteAsync(Guid id);
        Task<List<MovieDto>?> GetAllAsync();
        Task<HttpResponseMessage> UpdateAsync(Guid id, MovieDto movieToUpdate);
    }
}