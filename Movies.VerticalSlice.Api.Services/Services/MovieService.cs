using Movies.VerticalSlice.Api.Shared.Constants;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Movies.VerticalSlice.Api.Shared.Requests;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Movies.VerticalSlice.Api.Services;

public class MovieService : IMovieService
{
    private readonly HttpClient _httpClient;

    public string? AuthToken { get; set; }

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
        SetAuth();

        var movieToCreate = new CreateMovieRequest(
            movie.Title,
            movie.YearOfRelease,
            movie.Genres
        );
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Movies.Create, movieToCreate);
        CheckResponse(response);
        return response;
    }

    
    public async Task<HttpResponseMessage> UpdateAsync(Guid id, MovieDto movieToUpdate)
    {
        SetAuth();

        var updateRequest = new UpdateMovieRequest(movieToUpdate.Title,
          movieToUpdate.YearOfRelease,
          movieToUpdate.Genres
        );

        var url = $"{ApiEndpoints.Movies.Update}?id={id}";
        var response = await _httpClient.PutAsJsonAsync(url, updateRequest);
        CheckResponse(response);
        return response;

    }

    public async Task<HttpResponseMessage> DeleteAsync(Guid id)
    {
        SetAuth();
        var url = $"{ApiEndpoints.Movies.Delete}?id={id}";
        Console.WriteLine($"DeleteAsync URL: {_httpClient.BaseAddress}{url}");
        var response = await _httpClient.DeleteAsync(url);

        CheckResponse(response);
        return response;

    }

    void CheckResponse(HttpResponseMessage response)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("User is not authorized.");
        }
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            throw new InvalidOperationException("Bad request.");
        }
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException("Not Found");
        }
    }


    void SetAuth()
    {
        if (!string.IsNullOrEmpty(AuthToken))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
    }
}