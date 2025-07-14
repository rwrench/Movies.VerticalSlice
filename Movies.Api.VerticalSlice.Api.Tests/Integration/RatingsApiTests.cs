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
        IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        string baseUrl = "/api/movies/ratings";
        private string? _token;
        public RatingsApiTests(WebApplicationFactory<Program> factory)
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
            var response = await _client.GetAsync(baseUrl);
            response.EnsureSuccessStatusCode();

            var ratings = await response.Content.ReadFromJsonAsync<MovieRatingWithNameDto[]>();
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

        [Fact]
        public async Task CreateRating_ReturnsCreated()
        {
            var movieId = await GetMovieId();
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
        public async Task UpdateRating_ReturnsNoContent()
        {
            // First, create a rating
            var rating = new MovieRatingWithNameDto
            {
                Id = Guid.NewGuid(),
                MovieId = Guid.NewGuid(),
                Rating = 3.0f,
                UserId = "update-user",
                DateUpdated = DateTime.UtcNow,
                MovieName = "Update Movie",
                Genres = "Action",
                UserName = "Update User"
            };
            await _client.PostAsJsonAsync(baseUrl, rating);

            // Update the rating
            rating.Rating = 5.0f;
            var response = await _client.PutAsJsonAsync($"{baseUrl}/{rating.Id}", rating);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteRating_ReturnsOK()
        {
            var ratingId = Guid.NewGuid();  
            
            var rating = new MovieRatingWithNameDto
            {
                Id = ratingId,
                MovieId = Guid.NewGuid(),
                Rating = 2.0f,
                UserId = "delete-user",
                DateUpdated = DateTime.UtcNow,
                MovieName = "Delete Movie",
                Genres = "Comedy",
                UserName = "Delete User"
            };
            var response = await _client.PostAsJsonAsync("/api/movies/ratings", rating);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        
             Console.WriteLine($"{baseUrl}/{ratingId}");

            response = await _client.DeleteAsync($"{baseUrl}/{ratingId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

       

        public class LoginResponseDto
        {
            public required string Token { get; set; }
        }
    }
}
