using FluentAssertions;
using Movies.VerticalSlice.Api;
using Movies.VerticalSlice.Api.Shared.Constants;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Movies.VerticalSlice.Api.Shared.Requests;
using Movies.VerticalSlice.Api.Shared.Responses;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Movies.Api.VerticalSlice.Api.Tests.Integration
{
    public class MoviesApiTests :
        IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private string? _token;
        private readonly string baseUrl = ApiEndpoints.Movies.Base;

        public MoviesApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            _token = await GetJwtTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        [Fact]
        public async Task CreateMovie_ReturnsCreated()
        {
            var uniqueTitle = $"TestMovie_{Guid.NewGuid()}";
            var createMovieRequest = new CreateMovieRequest(uniqueTitle, 2025, "Test");
            var response = await _client.PostAsJsonAsync(baseUrl, createMovieRequest);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task GetAllMovies_ReturnsSuccessAndList()
        {
            await GivenWeHaveMovie();
            var response = await _client.GetAsync(baseUrl);
            response.EnsureSuccessStatusCode();

            var movies = await response.Content.ReadFromJsonAsync<MovieNameDto[]>();
            movies.Should().NotBeNull();
            movies.Should().NotBeEmpty();
        }

        [Fact]
        public async Task UpdateMovie_ReturnsNoContent()
        {
            var createdMovieId = await GivenWeHaveMovie();
            var title = $"UpdatedMovie_{Guid.NewGuid()}";
            var updateRequest = new UpdateMovieRequest(title, 2026, "UpdatedGenre");
            var response = await _client.PutAsJsonAsync($"{baseUrl}/{createdMovieId}", updateRequest);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteMovie_ReturnsOk()
        {
            var createdMovieId = await GivenWeHaveMovie();
            var response = await _client.DeleteAsync($"{baseUrl}/{createdMovieId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private async Task<Guid> GivenWeHaveMovie()
        {
            var uniqueTitle = $"TestMovie_{Guid.NewGuid()}";
            var createMovieRequest = new CreateMovieRequest(uniqueTitle, 2025, "Test");
            var response = await _client.PostAsJsonAsync(baseUrl, createMovieRequest);
            response.EnsureSuccessStatusCode();
            var createdMovie = await response.Content.ReadFromJsonAsync<CreatedIdResponse>();
            return createdMovie!.Id;
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public async Task<string> GetJwtTokenAsync()
        {
            var loginRequest = new
            {
                Email = "testuser@email.com",
                Password = "TestPassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/users/login", loginRequest);
            response.EnsureSuccessStatusCode();

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            return loginResponse!.Token;
        }

        public class LoginResponseDto
        {
            public required string Token { get; set; }
        }
    }
}