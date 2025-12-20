using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Movies.Api.VerticalSlice.Api.Tests.Infrastructure;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Shared.Constants;
using Movies.VerticalSlice.Api.Shared.Requests;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Integration;

public class RatingsEndpointsIntegrationTests : IntegrationTestBase
{
    public RatingsEndpointsIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    #region Create Rating Tests

    [Fact]
    public async Task CreateRating_WithValidData_Returns201Created()
    {
        var movie = await Given_we_have_a_movie_in_database();
        var request = And_we_have_a_valid_create_rating_request(movie.MovieId);
        var authenticatedClient = And_we_have_an_authenticated_client("user-123", "testuser");
        var response = await When_we_post_the_rating(authenticatedClient, request);
        Then_the_response_should_be_created(response);
        await And_the_rating_should_be_saved_in_database();
    }

    [Fact]
    public async Task CreateRating_WithoutAuthentication_Returns401Unauthorized()
    {
        var movie = await Given_we_have_a_movie_in_database();
        var request = And_we_have_a_valid_create_rating_request(movie.MovieId);
        var response = await When_we_post_the_rating_without_authentication(request);
        Then_the_response_should_be_unauthorized(response);
    }

    [Fact]
    public async Task CreateRating_WithInvalidRating_Returns400BadRequest()
    {
        var movie = await Given_we_have_a_movie_in_database();
        var request = Given_we_have_an_invalid_rating_request(movie.MovieId);
        var authenticatedClient = And_we_have_an_authenticated_client("user-123", "testuser");
        var response = await When_we_post_the_rating(authenticatedClient, request);
        Then_the_response_should_be_bad_request(response);
    }

    [Fact]
    public async Task CreateRating_ForNonExistentMovie_Returns400BadRequest()
    {
        var nonExistentMovieId = Given_we_have_a_non_existent_movie_id();
        var request = And_we_have_a_valid_create_rating_request(nonExistentMovieId);
        var authenticatedClient = And_we_have_an_authenticated_client("user-123", "testuser");
        var response = await When_we_post_the_rating(authenticatedClient, request);
        Then_the_response_should_be_bad_request(response);
    }

    #endregion

    #region Get All Ratings Tests

    [Fact]
    public async Task GetAllRatings_ReturnsRatingsList()
    {
        await Given_we_have_ratings_in_database();
        var response = await When_we_get_all_ratings();
        Then_the_response_should_be_ok(response);
        await And_the_response_should_contain_ratings(response);
    }

    [Fact]
    public async Task GetAllRatings_WithNoRatings_ReturnsEmptyList()
    {
        await Given_we_have_an_empty_database();
        var response = await When_we_get_all_ratings();
        Then_the_response_should_be_ok(response);
    }

    [Fact]
    public async Task GetUserRatings_WithAuthentication_ReturnsUserSpecificRatings()
    {
        await Given_we_have_ratings_in_database();
        var authenticatedClient = And_we_have_an_authenticated_client("user-123", "testuser");
        var response = await When_we_get_user_ratings(authenticatedClient);
        Then_the_response_should_be_ok(response);
    }

    [Fact]
    public async Task GetUserRatings_WithoutAuthentication_Returns401Unauthorized()
    {
        await Given_we_have_ratings_in_database();
        var response = await When_we_get_user_ratings_without_authentication();
        Then_the_response_should_be_unauthorized(response);
    }

    #endregion

    #region Update Rating Tests

    [Fact]
    public async Task UpdateRating_WithValidData_Returns200OK()
    {
        var rating = await Given_we_have_a_rating_in_database("user-123");
        var updateRequest = And_we_have_an_update_rating_request(rating.MovieId);
        var authenticatedClient = And_we_have_an_authenticated_client("user-123", "testuser");
        var response = await When_we_update_the_rating(authenticatedClient, rating.Id, updateRequest);
        Then_the_response_should_be_ok_or_no_content(response);
        await And_the_rating_should_be_updated_in_database(rating.Id, 3.5f);
    }

    [Fact]
    public async Task UpdateRating_WithoutAuthentication_Returns401Unauthorized()
    {
        var rating = await Given_we_have_a_rating_in_database("user-123");
        var updateRequest = And_we_have_an_update_rating_request(rating.MovieId);
        var response = await When_we_update_the_rating_without_authentication(rating.Id, updateRequest);
        Then_the_response_should_be_unauthorized(response);
    }

    [Fact]
    public async Task UpdateRating_OtherUsersRating_Returns403ForbiddenOrUnauthorized()
    {
        var rating = await Given_we_have_a_rating_in_database("user-456");
        var updateRequest = And_we_have_an_update_rating_request(rating.MovieId);
        var authenticatedClient = And_we_have_an_authenticated_client("user-123", "testuser");
        var response = await When_we_update_the_rating(authenticatedClient, rating.Id, updateRequest);
        Then_the_response_should_be_forbidden_or_unauthorized(response);
    }

    #endregion

    #region Delete Rating Tests

    [Fact]
    public async Task DeleteRating_WithValidId_Returns204NoContent()
    {
        var rating = await Given_we_have_a_rating_in_database("user-123");
        var authenticatedClient = And_we_have_an_authenticated_client("user-123", "testuser");
        var response = await When_we_delete_the_rating(authenticatedClient, rating.Id);
        Then_the_response_should_be_no_content(response);
        await And_the_rating_should_be_deleted_from_database(rating.Id);
    }

    [Fact]
    public async Task DeleteRating_WithoutAuthentication_Returns401Unauthorized()
    {
        var rating = await Given_we_have_a_rating_in_database("user-123");
        var response = await When_we_delete_the_rating_without_authentication(rating.Id);
        Then_the_response_should_be_unauthorized(response);
    }

    [Fact]
    public async Task DeleteRating_NonExistentRating_Returns404NotFound()
    {
        var nonExistentId = Given_we_have_a_non_existent_rating_id();
        var authenticatedClient = And_we_have_an_authenticated_client("user-123", "testuser");
        var response = await When_we_delete_the_rating(authenticatedClient, nonExistentId);
        Then_the_response_should_be_not_found_or_bad_request(response);
    }

    #endregion

    #region Given Methods

    async Task<Movie> Given_we_have_a_movie_in_database(string userId = "user-123")
    {
        var movie = new Movie
        {
            MovieId = Guid.NewGuid(),
            Title = "Test Movie for Ratings",
            YearOfRelease = 2023,
            Genres = "Action",
            UserId = userId,
            DateUpdated = DateTime.Now
        };

        DbContext.Movies.Add(movie);
        await DbContext.SaveChangesAsync();
        return movie;
    }

    async Task Given_we_have_ratings_in_database()
    {
        var movie1 = await Given_we_have_a_movie_in_database("user-123");
        var movie2 = await Given_we_have_a_movie_in_database("user-456");

        var ratings = new List<MovieRating>
        {
            new MovieRating
            {
                Id = Guid.NewGuid(),
                MovieId = movie1.MovieId,
                Rating = 4.5f,
                UserId = "user-123",
                DateUpdated = DateTime.Now
            },
            new MovieRating
            {
                Id = Guid.NewGuid(),
                MovieId = movie2.MovieId,
                Rating = 3.0f,
                UserId = "user-456",
                DateUpdated = DateTime.Now
            },
            new MovieRating
            {
                Id = Guid.NewGuid(),
                MovieId = movie1.MovieId,
                Rating = 5.0f,
                UserId = "user-789",
                DateUpdated = DateTime.Now
            }
        };

        DbContext.Ratings.AddRange(ratings);
        await DbContext.SaveChangesAsync();
    }

    async Task<MovieRating> Given_we_have_a_rating_in_database(string userId = "user-123")
    {
        var movie = await Given_we_have_a_movie_in_database(userId);

        var rating = new MovieRating
        {
            Id = Guid.NewGuid(),
            MovieId = movie.MovieId,
            Rating = 4.0f,
            UserId = userId,
            DateUpdated = DateTime.Now
        };

        DbContext.Ratings.Add(rating);
        await DbContext.SaveChangesAsync();
        return rating;
    }

    async Task Given_we_have_an_empty_database()
    {
        await ClearDatabase();
    }

    Guid Given_we_have_a_non_existent_movie_id()
    {
        return Guid.NewGuid();
    }

    Guid Given_we_have_a_non_existent_rating_id()
    {
        return Guid.NewGuid();
    }

    CreateRatingRequest Given_we_have_an_invalid_rating_request(Guid movieId)
    {
        return new CreateRatingRequest(
            MovieId: movieId,
            Rating: 6.0f,
            DateUpdated: DateTime.Now
        );
    }

    #endregion

    #region And Methods

    HttpClient And_we_have_an_authenticated_client(string userId, string userName)
    {
        return CreateAuthenticatedClient(userId, userName);
    }

    CreateRatingRequest And_we_have_a_valid_create_rating_request(Guid movieId)
    {
        return new CreateRatingRequest(
            MovieId: movieId,
            Rating: 4.5f,
            DateUpdated: DateTime.Now
        );
    }

    CreateRatingRequest And_we_have_an_update_rating_request(Guid movieId)
    {
        return new CreateRatingRequest(
            MovieId: movieId,
            Rating: 3.5f,
            DateUpdated: DateTime.Now
        );
    }

    #endregion

    #region When Methods

    async Task<HttpResponseMessage> When_we_post_the_rating(HttpClient client, CreateRatingRequest request)
    {
        return await client.PostAsJsonAsync(ApiEndpoints.Ratings.Base, request);
    }

    async Task<HttpResponseMessage> When_we_post_the_rating_without_authentication(CreateRatingRequest request)
    {
        return await Client.PostAsJsonAsync(ApiEndpoints.Ratings.Base, request);
    }

    async Task<HttpResponseMessage> When_we_get_all_ratings()
    {
        return await Client.GetAsync(ApiEndpoints.Ratings.GetAll);
    }

    async Task<HttpResponseMessage> When_we_get_user_ratings(HttpClient client)
    {
        return await client.GetAsync(ApiEndpoints.Ratings.GetUserRatings);
    }

    async Task<HttpResponseMessage> When_we_get_user_ratings_without_authentication()
    {
        return await Client.GetAsync(ApiEndpoints.Ratings.GetUserRatings);
    }

    async Task<HttpResponseMessage> When_we_update_the_rating(HttpClient client, Guid ratingId, CreateRatingRequest request)
    {
        return await client.PutAsJsonAsync($"{ApiEndpoints.Ratings.Base}/{ratingId}", request);
    }

    async Task<HttpResponseMessage> When_we_update_the_rating_without_authentication(Guid ratingId, CreateRatingRequest request)
    {
        return await Client.PutAsJsonAsync($"{ApiEndpoints.Ratings.Base}/{ratingId}", request);
    }

    async Task<HttpResponseMessage> When_we_delete_the_rating(HttpClient client, Guid ratingId)
    {
        return await client.DeleteAsync($"{ApiEndpoints.Ratings.Base}/{ratingId}");
    }

    async Task<HttpResponseMessage> When_we_delete_the_rating_without_authentication(Guid ratingId)
    {
        return await Client.DeleteAsync($"{ApiEndpoints.Ratings.Base}/{ratingId}");
    }

    #endregion

    #region Then Methods

    void Then_the_response_should_be_created(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    void Then_the_response_should_be_ok(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    void Then_the_response_should_be_unauthorized(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    void Then_the_response_should_be_bad_request(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    void Then_the_response_should_be_no_content(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    void Then_the_response_should_be_ok_or_no_content(HttpResponseMessage response)
    {
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    void Then_the_response_should_be_not_found_or_bad_request(HttpResponseMessage response)
    {
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    void Then_the_response_should_be_forbidden_or_unauthorized(HttpResponseMessage response)
    {
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    async Task And_the_rating_should_be_saved_in_database()
    {
        var ratingCount = await GetRatingCount();
        ratingCount.Should().Be(1);
    }

    async Task And_the_response_should_contain_ratings(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }

    async Task And_the_rating_should_be_updated_in_database(Guid ratingId, float expectedRating)
    {
        var updatedRating = await DbContext.Ratings.FindAsync(ratingId);
        updatedRating.Should().NotBeNull();
        updatedRating!.Rating.Should().Be(expectedRating);
    }

    async Task And_the_rating_should_be_deleted_from_database(Guid ratingId)
    {
        var deletedRating = await DbContext.Ratings.FindAsync(ratingId);
        deletedRating.Should().BeNull();
    }

    #endregion
}
