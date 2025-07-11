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
            var ratingsToCreate = new RateMovieRequest(
                rating.MovieId,
                rating.Rating,
                rating.DateUpdated
            );

            return await _httpClient.PostAsJsonAsync("api/movies/ratings", ratingsToCreate);
        }


        public async Task<HttpResponseMessage> DeleteAsync(Guid id)
        {
            return await _httpClient.DeleteAsync($"api/ratings/{id}");
        }


    }
}
