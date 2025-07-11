using System.Net.Http.Json;
using Movies.VerticalSlice.Api.Features.Movies.GetAll;
using System.Net.Http.Headers;

namespace Movies.VerticalSlice.Api.Blazor.Services;

public class MovieService
{
    private readonly HttpClient _httpClient;

    public MovieService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<MovieDto>?> GetAllAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<MovieDto>>("api/movies");
    }

    public async Task<MovieDto?> GetByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<MovieDto>($"api/movies/{id}");
    }

    public async Task<HttpResponseMessage> CreateAsync(object createRequest)
    {
        return await _httpClient.PostAsJsonAsync("api/movies", createRequest);
    }

    public async Task<HttpResponseMessage> UpdateAsync(Guid id, object updateRequest)
    {
        return await _httpClient.PutAsJsonAsync($"api/movies/{id}", updateRequest);
    }

    public async Task<HttpResponseMessage> DeleteAsync(Guid id)
    {
        return await _httpClient.DeleteAsync($"api/movies/{id}");
    }
}