using Movies.VerticalSlice.Api.Shared.Constants;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Movies.VerticalSlice.Api.Shared.Requests;
using System.Net.Http;
using System.Net.Http.Json;

namespace Movies.VerticalSlice.Api.Services
{
    public class MovieService
    {
        private readonly HttpClient _httpClient;

        public MovieService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AuthorizedClient");
        }

        public async Task<List<MovieDto>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<MovieDto>>(ApiEndpoints.Movies.GetAll);
        }

        public async Task<HttpResponseMessage> CreateAsync(MovieDto movie)
        {
            var movieToCreate = new CreateMovieRequest(
                movie.Title,
                movie.YearOfRelease,
                movie.Genres
            );  
           
            return await _httpClient.PostAsJsonAsync(ApiEndpoints.Movies.Create, movieToCreate);
        }

        public async Task<HttpResponseMessage> UpdateAsync(Guid id, MovieDto movieToUpdate)
        {
            var updateRequest = new UpdateMovieRequest(movieToUpdate.Title,
              movieToUpdate.YearOfRelease,
              movieToUpdate.Genres  
            );
            return await _httpClient.PutAsJsonAsync(ApiEndpoints.Movies.Update, updateRequest);
        }

        public async Task<HttpResponseMessage> DeleteAsync(Guid id)
        {
            return await _httpClient.DeleteAsync(ApiEndpoints.Movies.Delete);
        }

      
    }
}
