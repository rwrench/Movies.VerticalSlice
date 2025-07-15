using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Movies.VerticalSlice.Api;
using Movies.VerticalSlice.Api.Shared.Dtos;
using Movies.VerticalSlice.Api.Shared.Requests;
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

        public Task DisposeAsync() => Task.CompletedTask;

        public async Task<string> GetJwtTokenAsync()
        {
            var loginRequest = new
            {
                Email = "rwrench@gmail.com",
                Password = "Kerry!1234"
            };

            var response = await _client.PostAsJsonAsync("/api/users/login", loginRequest);
            response.EnsureSuccessStatusCode();

            // Adjust the property name to match your actual response
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            return loginResponse!.Token;
        }

        [Fact]
        public async Task GetAllRatings_Authorized_ReturnsSuccessAndList()
        {
            MovieRatingWithNameDto[]? ratings = await GivenWeHaveRatings();
            ratings.Should().NotBeNull();
        }


        public async Task<Guid> GetMovieId()
        {
            var response = await _client.GetAsync("/api/movies/names/");
            response.EnsureSuccessStatusCode();
            var movies = await response.Content.ReadFromJsonAsync<List<MovieNameDto>>();
            movies.Should().NotBeNullOrEmpty("No movies found in the database for testing.");
            return movies![0].Id;
        }

       
        public async Task<Guid> GetMovieIdWithNoRatingsAsync()
        {
            var response = await _client.GetAsync("/api/movies/names/");
            response.EnsureSuccessStatusCode();
            var movies = await response.Content.ReadFromJsonAsync<List<MovieNameDto>>();
            movies.Should().NotBeNullOrEmpty("No movies found in the database for testing.");

            response = await _client.GetAsync(baseUrl);
            var ratings = await response.Content.ReadFromJsonAsync<MovieRatingWithNameDto[]>();
            ratings.Should().NotBeNullOrEmpty("No ratings found in the database for testing.");
            
            var id = movies.First(x => !ratings.Any(r => r.MovieId == x.Id)).Id; 
            id.Should().NotBeEmpty("No movies found without ratings for testing."); 
            return id;
        }


        [Fact]
        public async Task CreateRating_ReturnsCreated()
        {
            var movieId = await GetMovieIdWithNoRatingsAsync();
            var newRating = new CreateRatingRequest(
                movieId,
                4.5f,
                DateTime.UtcNow
            );


            var response = await _client.PostAsJsonAsync(baseUrl, newRating);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorContent}");
            }
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task UpdateRating_Returns_NO_Content()
        {
            var response = await _client.GetAsync(baseUrl);
            response.EnsureSuccessStatusCode();

            var ratings = await response.Content.ReadFromJsonAsync<MovieRatingWithNameDto[]>();
            var rating = ratings!.ToList().OrderByDescending(x => x.DateUpdated).First();
            // Update the rating

            UpdateRatingsRequest req = new UpdateRatingsRequest(rating.MovieId, 4, DateTime.Now);

            response = await _client.PutAsJsonAsync($"{baseUrl}/{rating.Id}", req);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteRating_ReturnsOK()
        {
            MovieRatingWithNameDto[]? ratings = await GivenWeHaveRatings();
            var id = ratings!.First().Id;
            HttpResponseMessage response = await WhenWeDeleteARating(id);
            ThenTheRatingShouldDelete(response);

        }

        void ThenTheRatingShouldDelete(HttpResponseMessage response)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        async Task<HttpResponseMessage> WhenWeDeleteARating(Guid id)
        {
            return await _client.DeleteAsync($"/api/movies/ratings/{id}");
        }

        async Task<MovieRatingWithNameDto[]?> GivenWeHaveRatings()
        {
            var response = await _client.GetAsync(baseUrl);
            response.EnsureSuccessStatusCode();

            var ratings = await response.Content.ReadFromJsonAsync<MovieRatingWithNameDto[]>();
            return ratings;
        }

        public class LoginResponseDto
        {
            public required string Token { get; set; }
        }
    }
}
