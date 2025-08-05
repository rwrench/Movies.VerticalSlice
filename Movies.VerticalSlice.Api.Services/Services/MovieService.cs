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
        public string? AuthToken;

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
            if (!string.IsNullOrEmpty(AuthToken))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthToken);

            var movieToCreate = new CreateMovieRequest(
                movie.Title,
                movie.YearOfRelease,
                movie.Genres
            );
            var response =  await _httpClient.PostAsJsonAsync(ApiEndpoints.Movies.Create, movieToCreate);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("User is not authorized.");
            }

            return response;
        }

        public async Task<HttpResponseMessage> UpdateAsync(Guid id, MovieDto movieToUpdate)
        {
            if (!string.IsNullOrEmpty(AuthToken))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthToken);

            var updateRequest = new UpdateMovieRequest(movieToUpdate.Title,
              movieToUpdate.YearOfRelease,
              movieToUpdate.Genres  
            );
            var response = await _httpClient.PutAsJsonAsync(ApiEndpoints.Movies.Update, updateRequest);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("User is not authorized.");
            }

            return response;

        }

        public async Task<HttpResponseMessage> DeleteAsync(Guid id)
        {
            if (!string.IsNullOrEmpty(AuthToken))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthToken);

            var url = $"{ApiEndpoints.Movies.Delete}/{id}";
            Console.WriteLine($"DeleteAsync URL: {_httpClient.BaseAddress}{url}");
            var response = await _httpClient.DeleteAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("User is not authorized.");
            }
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException("Bad request.");
            }
            return response;

        }

       
    }
}
