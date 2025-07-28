using Movies.VerticalSlice.Api.Shared.Dtos;
using Movies.VerticalSlice.Api.Shared.Requests;
using System.Net.Http.Json;

namespace Movies.VerticalSlice.Api.Blazor.Services
{
    public class RatingsService
    {
        private readonly HttpClient _httpClient;

        public RatingsService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AuthorizedClient");
        }

        public async Task<List<MovieRatingWithNameDto>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<MovieRatingWithNameDto>>("api/movies/ratings");
        }

        public async Task<HttpResponseMessage> CreateAsync(MovieRatingWithNameDto rating)
        {
            var ratingsToCreate = new CreateRatingRequest(
                rating.MovieId,
                rating.Rating,
                rating.DateUpdated
            );

            return await _httpClient.PostAsJsonAsync("api/movies/ratings", ratingsToCreate);
        }

        public async Task<HttpResponseMessage> UpdateAsync(
            MovieRatingWithNameDto rating)
        {

            var ratingsToUpdate = new UpdateRatingsRequest(
                rating.MovieId,
                rating.Rating,
                rating.DateUpdated
            );

            return await _httpClient.PutAsJsonAsync($"api/movies/ratings/{rating.Id}",
                ratingsToUpdate);
        }

        public async Task<HttpResponseMessage> DeleteAsync(Guid id)
        {
            return await _httpClient.DeleteAsync($"api/movies/ratings/{id}");
        }

        public async Task<List<MovieNameDto>?> GetAllMovieNamesAsync(string title)
        {
            var url = string.IsNullOrWhiteSpace(title)
                 ? "api/movies/names/"
                 : $"api/movies/names?title={Uri.EscapeDataString(title)}";

            var result = await _httpClient.GetFromJsonAsync<List<MovieNameDto>>(url);
            return result ?? new List<MovieNameDto>();
        }
    }
}
