using Movies.VerticalSlice.Api.Shared.Constants;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Movies.VerticalSlice.Api.Shared.Requests;
using System.Net.Http.Json;

namespace Movies.VerticalSlice.Api.Services
{
    public class RatingsService
    {
        private readonly HttpClient _httpClient;
        public string? AuthToken { get; set; } // Set this after login

        public RatingsService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AuthorizedClient");

        }

        public async Task<List<MovieRatingWithNameDto>?> GetAllAsync()
        {
            if (!string.IsNullOrEmpty(AuthToken))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthToken);

            var response = await _httpClient.GetAsync(ApiEndpoints.Ratings.GetAll);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<MovieRatingWithNameDto>>();
            return null;
        }

        public async Task<HttpResponseMessage> CreateAsync(MovieRatingWithNameDto rating)
        {
            var ratingsToCreate = new CreateRatingRequest(
                rating.MovieId,
                rating.Rating,
                rating.DateUpdated
            );

            return await _httpClient.PostAsJsonAsync(ApiEndpoints.Ratings.Base, ratingsToCreate);
        }

        public async Task<HttpResponseMessage> UpdateAsync(
            MovieRatingWithNameDto rating)
        {

            var ratingsToUpdate = new UpdateRatingsRequest(
                rating.MovieId,
                rating.Rating,
                rating.DateUpdated
            );

            return await _httpClient.PutAsJsonAsync($"{ApiEndpoints.Ratings.Base}/{rating.Id}",
                ratingsToUpdate);
        }

        public async Task<HttpResponseMessage> DeleteAsync(Guid id)
        {
            return await _httpClient.DeleteAsync($"{ApiEndpoints.Ratings.Base}/{id}");
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
