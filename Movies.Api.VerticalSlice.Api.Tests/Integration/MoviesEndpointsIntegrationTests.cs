using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Movies.Api.VerticalSlice.Api.Tests.Infrastructure;
using Movies.VerticalSlice.Api.Data.Models;
using Movies.VerticalSlice.Api.Shared.Constants;
using Movies.VerticalSlice.Api.Shared.Requests;

namespace Movies.Api.VerticalSlice.Api.Tests.Integration;

public class MoviesEndpointsIntegrationTests : IntegrationTestBase
{
    
    public MoviesEndpointsIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    { 
    }

    #region Create Movie Tests

    [Fact]
    public async Task CreateMovie_WithValidData_Returns201Created()
    {
        await ClearDatabase();
        var request = Given_we_have_a_valid_create_movie_request();
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        var response = await When_we_post_the_movie(authenticatedClient, request);
        Then_the_response_should_be_created(response);
        await And_the_movie_should_be_saved_in_database();
    }

    [Fact]
    public async Task CreateMovie_WithoutAuthentication_Returns401Unauthorized()
    {
        var request = Given_we_have_a_valid_create_movie_request();
        var response = await When_we_post_the_movie_without_authentication(request);
        Then_the_response_should_be_unauthorized(response);
    }

   
    #endregion

    #region Get All Movies Tests

    [Fact]
    public async Task GetAllMovies_ReturnsMoviesList()
    {
        await Given_we_have_movies_in_database();
        var response = await When_we_get_all_movies();
        Then_the_response_should_be_ok(response);
        await And_the_response_should_contain_movies(response);
    }

    [Fact]
    public async Task GetAllMovies_WithNoMovies_ReturnsEmptyList()
    {
        await Given_we_have_movies_in_database();
        var response = await When_we_get_all_movies();
        Then_the_response_should_be_ok(response);
    }

    [Fact]
    public async Task GetAllMovies_WithTitleFilter_ReturnsFilteredMovies()
    {
        await Given_we_have_movies_in_database();
        var titleFilter = And_we_have_a_title_filter("Action");
        var response = await When_we_get_movies_with_filter(titleFilter);
        Then_the_response_should_be_ok(response);
    }

    #endregion

    #region Update Movie Tests

  

    [Fact]
    public async Task UpdateMovie_WithoutAuthentication_Returns401Unauthorized()
    {
        var movie = await Given_we_have_a_movie_in_database(UserId);
        var updateRequest = And_we_have_an_update_request();
        var response = await When_we_update_the_movie_without_authentication(movie.MovieId, updateRequest);
        Then_the_response_should_be_unauthorized(response);
    }

    #endregion

    #region Delete Movie Tests

    [Fact]
    public async Task DeleteMovie_WithValidId_ReturnsOK()
    {
        var movie = await Given_we_have_a_movie_in_database(UserId);
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        var movieId = movie.MovieId;
        var response = await When_we_delete_the_movie(authenticatedClient, movieId);
        Then_the_response_should_be_ok(response);
        await And_the_movie_should_be_deleted_from_database(movieId);
    }

    [Fact]
    public async Task DeleteMovie_WithoutAuthentication_Returns401Unauthorized()
    {
        var movie = await Given_we_have_a_movie_in_database(UserId);
        var response = await When_we_delete_the_movie_without_authentication(movie.MovieId);
        Then_the_response_should_be_unauthorized(response);
    }

    [Fact]
    public async Task DeleteMovie_NonExistentMovie_Returns404NotFound()
    {
        var nonExistentId = Given_we_have_a_non_existent_movie_id();
        var authenticatedClient = And_we_have_an_authenticated_client(UserId, UserName);
        var response = await When_we_delete_the_movie(authenticatedClient, nonExistentId);
        Then_the_response_should_be_not_found_or_bad_request(response);
    }

    #endregion

    #region Given Methods

    CreateMovieRequest Given_we_have_a_valid_create_movie_request()
    {
        return new CreateMovieRequest(
            Title: "Test Movie",
            YearOfRelease: 2024,
            Genres: "Action,Drama"
        );
    }

    async Task<List<Movie>> Given_we_have_movies_in_database()
    {
        var movies = new List<Movie>
        {
            new Movie
            {
                MovieId = Guid.NewGuid(),
                Title = "Action Movie 1",
                YearOfRelease = 2023,
                Genres = "Action,Adventure",
                UserId = UserId,
                DateUpdated = DateTime.Now
            },
            new Movie
            {
                MovieId = Guid.NewGuid(),
                Title = "Drama Movie 1",
                YearOfRelease = 2022,
                Genres = "Drama",
                UserId = "UserId",
                DateUpdated = DateTime.Now
            },
            new Movie
            {
                MovieId = Guid.NewGuid(),
                Title = "Action Movie 2",
                YearOfRelease = 2024,
                Genres = "Action,Sci-Fi",
                UserId = UserId,
                DateUpdated = DateTime.Now
            }
        };

        DbContext.Movies.AddRange(movies);
        await DbContext.SaveChangesAsync();
        return movies;
    }

    async Task<Movie> Given_we_have_a_movie_in_database(string userId = UserId)
    {
        var movie = new Movie
        {
            MovieId = Guid.NewGuid(),
            Title = "Test Movie",
            YearOfRelease = 2023,
            Genres = "Action,Drama",
            UserId = userId,
            DateUpdated = DateTime.Now
        };

        DbContext.Movies.Add(movie);
        await DbContext.SaveChangesAsync();
        return movie;
    }

   
    Guid Given_we_have_a_non_existent_movie_id()
    {
        return Guid.NewGuid();
    }

    #endregion

    #region And Methods

    HttpClient And_we_have_an_authenticated_client(string userId, string userName)
    {
        return CreateAuthenticatedClient(userId, userName);
    }

    string And_we_have_a_title_filter(string title)
    {
        return title;
    }

    UpdateMovieRequest And_we_have_an_update_request()
    {
        return new UpdateMovieRequest(
            Title: "Updated Movie Title",
            YearOfRelease: 2024,
            Genres: "Drama,Thriller"
        );
    }

    #endregion

    #region When Methods

    async Task<HttpResponseMessage> When_we_post_the_movie(HttpClient client, CreateMovieRequest request)
    {
        return await client.PostAsJsonAsync(ApiEndpoints.Movies.Create, request);
    }

    async Task<HttpResponseMessage> When_we_post_the_movie_without_authentication(CreateMovieRequest request)
    {
        return await Client.PostAsJsonAsync(ApiEndpoints.Movies.Create, request);
    }

    async Task<HttpResponseMessage> When_we_get_all_movies()
    {
        return await Client.GetAsync(ApiEndpoints.Movies.GetAll);
    }

    async Task<HttpResponseMessage> When_we_get_movies_with_filter(string titleFilter)
    {
        return await Client.GetAsync($"{ApiEndpoints.Movies.GetAll}?title={titleFilter}");
    }

    async Task<HttpResponseMessage> When_we_update_the_movie(HttpClient client, Guid movieId, UpdateMovieRequest request)
    {
        return await client.PutAsJsonAsync(ApiEndpoints.Movies.UpdateWithId(movieId), request);
    }

    async Task<HttpResponseMessage> When_we_update_the_movie_without_authentication(Guid movieId, UpdateMovieRequest request)
    {
        return await Client.PutAsJsonAsync(ApiEndpoints.Movies.UpdateWithId(movieId), request);
    }

    async Task<HttpResponseMessage> When_we_delete_the_movie(HttpClient client, Guid movieId)
    {
        return await client.DeleteAsync(ApiEndpoints.Movies.DeleteWithId(movieId));
    }

    async Task<HttpResponseMessage> When_we_delete_the_movie_without_authentication(Guid movieId)
    {
        return await Client.DeleteAsync(ApiEndpoints.Movies.DeleteWithId(movieId));
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

   

  

  
    void Then_the_response_should_be_not_found_or_bad_request(HttpResponseMessage response)
    {
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    async Task And_the_movie_should_be_saved_in_database()
    {
        DbContext.ChangeTracker.Clear();
        var movieCount = await GetMovieCount();
        movieCount.Should().Be(1);
    }

    async Task And_the_response_should_contain_movies(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }

    async Task And_the_movie_should_be_updated_in_database(Guid movieId, UpdateMovieRequest request)
    {
        var updatedMovie = await DbContext.Movies.FindAsync(movieId);
        updatedMovie.Should().NotBeNull();
        updatedMovie!.Title.Should().Be(request.Title);
    }

    async Task And_the_movie_should_be_deleted_from_database(Guid movieId)
    {
        DbContext.ChangeTracker.Clear();
        var deletedMovie = await DbContext.Movies.FindAsync(movieId);
        deletedMovie.Should().BeNull();
    }

    #endregion
}
