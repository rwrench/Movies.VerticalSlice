using FluentAssertions;
using Movies.VerticalSlice.Api;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Movies.VerticalSlice.Api.Shared.Requests;
using Movies.VerticalSlice.Api.Shared.Responses;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Movies.Api.VerticalSlice.Api.Tests.Integration
{
    public class RatingsApiTests :
        IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        string baseUrl = "/api/movies/ratings";
        private string? _token;
        public RatingsApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            
        }
        public async Task InitializeAsync()
        {
            _token = await GetJwtTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

      
      
        [Fact]
        public async Task GetAllRatings_Authorized_ReturnsSuccessAndList()
        {
            var movieId = await GivenWeHaveMovies();
            var newRating = new CreateRatingRequest(
                movieId,
                4.5f,
                DateTime.UtcNow
            );
            var response = await _client.PostAsJsonAsync(baseUrl, newRating);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response = await _client.GetAsync(baseUrl);
            response.EnsureSuccessStatusCode();

            var ratings = await response.Content.ReadFromJsonAsync<MovieRatingWithNameDto[]>();
            ratings.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateRating_ReturnsCreated()
        {
            var movieId = await GivenWeHaveMovies();
            var newRating = new CreateRatingRequest(
                movieId,
                4.5f,
                DateTime.UtcNow
            );
            var response = await _client.PostAsJsonAsync(baseUrl, newRating);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task UpdateRating_Returns_NO_Content()
        {
            var movieId = await GivenWeHaveMovies();
            var createdRating = await GivenWeHaveRatings();
            var updateRequest = new UpdateRatingsRequest(movieId, 4, DateTime.UtcNow);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var response = await _client.PutAsJsonAsync($"{baseUrl}/{createdRating!.Id}", updateRequest);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteRating_ReturnsOK()
        {
            CreatedIdResponse createdRating = await GivenWeHaveRatings();
            var response = await _client.DeleteAsync($"{baseUrl}/{createdRating!.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private async Task<CreatedIdResponse> GivenWeHaveRatings()
        {
            var movieId = await GivenWeHaveMovies();
            var newRating = new CreateRatingRequest(movieId, 2.0f, DateTime.UtcNow);
            var createResponse = await _client.PostAsJsonAsync(baseUrl, newRating);
            var createdRating = await createResponse.Content.ReadFromJsonAsync<CreatedIdResponse>();
            return createdRating!;
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

            // Adjust the property name to match your actual response
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            return loginResponse!.Token;
        }


        public class LoginResponseDto
        {
            public required string Token { get; set; }
        }

        async Task<Guid> GivenWeHaveMovies()
        {
            var uniqueTitle = $"TestMovie_{Guid.NewGuid()}";
            var createMovieRequest = new CreateMovieRequest(uniqueTitle, 2025, "Test");
            var response = await _client.PostAsJsonAsync("/api/movies", createMovieRequest);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<MovieNameDto>();
            return created!.Id;
        }


    }
}
