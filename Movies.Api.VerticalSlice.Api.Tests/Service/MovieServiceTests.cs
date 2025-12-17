using FluentAssertions;
using Moq;
using Moq.Protected;
using Movies.VerticalSlice.Api.Services;
using Movies.VerticalSlice.Api.Shared.Constants;
using Movies.VerticalSlice.Api.Shared.Dtos;
using System.Net;
using System.Text.Json;
using Xunit;
     
namespace Movies.Api.VerticalSlice.Api.Tests.Service;

public class MovieServiceTests
{
    #region Delete Tests
    
    [Fact]
    public async Task DeleteAsync_ReturnsUnauthorized_ThrowsException()
    {
        var movieId = Given_we_have_a_movie_id();
        var service = And_we_have_a_service_that_returns_unauthorized();
        await Then_unauthorized_exception_should_be_thrown(service, movieId);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_CallsCorrectEndpoint()
    {
        var movieId = Given_we_have_a_movie_id();
        var (service, requestCapture) = And_we_have_a_service_with_request_capture();
        await When_we_delete_the_movie(service, movieId);
        Then_correct_delete_endpoint_should_be_called(requestCapture, movieId);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsOK_CompletesSuccessfully()
    {
        var movieId = Given_we_have_a_movie_id();
        var service = And_we_have_a_service_that_returns_ok();
        var response = await When_we_delete_the_movie(service, movieId);
        Then_response_should_be_successful(response);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_WithValidId_CallsCorrectEndpoint()
    {
        var movieId = Given_we_have_a_movie_id();
        var movieDto = Given_we_have_a_movie_dto();
        var (service, requestCapture) = And_we_have_a_service_with_request_capture();
        await When_we_update_the_movie(service, movieId, movieDto);
        Then_correct_update_endpoint_should_be_called(requestCapture, movieId);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUnauthorized_ThrowsException()
    {
        var movieId = Given_we_have_a_movie_id();
        var movieDto = Given_we_have_a_movie_dto();
        var service = And_we_have_a_service_that_returns_unauthorized();
        await Then_unauthorized_exception_should_be_thrown_on_update(service, movieId, movieDto);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNoContent_CompletesSuccessfully()
    {
        var movieId = Given_we_have_a_movie_id();
        var movieDto = Given_we_have_a_movie_dto();
        var service = And_we_have_a_service_that_returns_no_content();
        var response = await When_we_update_the_movie(service, movieId, movieDto);
        Then_response_should_be_no_content(response);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task CreateAsync_WithValidMovie_CallsCorrectEndpoint()
    {
        var movieDto = Given_we_have_a_movie_dto();
        var (service, requestCapture) = And_we_have_a_service_with_request_capture_created();
        await When_we_create_the_movie(service, movieDto);
        Then_correct_create_endpoint_should_be_called(requestCapture);
    }

    [Fact]
    public async Task CreateAsync_ReturnsUnauthorized_ThrowsException()
    {
        var movieDto = Given_we_have_a_movie_dto();
        var service = And_we_have_a_service_that_returns_unauthorized();
        await Then_unauthorized_exception_should_be_thrown_on_create(service, movieDto);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreated_CompletesSuccessfully()
    {
        var movieDto = Given_we_have_a_movie_dto();
        var service = And_we_have_a_service_that_returns_created();
        var response = await When_we_create_the_movie(service, movieDto);
        Then_response_should_be_created(response);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAllAsync_CallsCorrectEndpoint()
    {
        var (service, requestCapture) = And_we_have_a_service_with_request_capture_and_json_response();
        await When_we_get_all_movies(service);
        Then_correct_getall_endpoint_should_be_called(requestCapture);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMovies_Successfully()
    {
        var expectedMovies = Given_we_have_a_list_of_movies();
        var service = And_we_have_a_service_that_returns_movies(expectedMovies);
        var movies = await When_we_get_all_movies(service);
        Then_movies_should_be_returned(movies, expectedMovies);
    }

    #endregion

    #region Given Methods

    Guid Given_we_have_a_movie_id()
    {
        return Guid.NewGuid();
    }

    MovieDto Given_we_have_a_movie_dto()
    {
        return new MovieDto
        {
            MovieId = Guid.NewGuid(),
            Title = "Test Movie",
            YearOfRelease = 2024,
            Slug = "test-movie",
            Genres = "Action,Drama"
        };
    }

    List<MovieDto> Given_we_have_a_list_of_movies()
    {
        return new List<MovieDto>
        {
            new MovieDto { MovieId = Guid.NewGuid(), Title = "Movie 1", Slug = "movie-1", YearOfRelease = 2023, Genres = "Action" },
            new MovieDto { MovieId = Guid.NewGuid(), Title = "Movie 2", Slug = "movie-2", YearOfRelease = 2024, Genres = "Drama" }
        };
    }

    #endregion

    #region And Methods

    MovieService And_we_have_a_service_that_returns_unauthorized()
    {
        var handler = CreateMockHttpHandler(HttpStatusCode.Unauthorized);
        return CreateMovieService(handler);
    }

    (MovieService service, RequestCapture requestCapture) And_we_have_a_service_with_request_capture()
    {
        var requestCapture = new RequestCapture();
        
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => requestCapture.CapturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var service = CreateMovieService(handler);
        
        return (service, requestCapture);
    }

    (MovieService service, RequestCapture requestCapture) And_we_have_a_service_with_request_capture_created()
    {
        var requestCapture = new RequestCapture();
        
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => requestCapture.CapturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

        var service = CreateMovieService(handler);
        
        return (service, requestCapture);
    }

    (MovieService service, RequestCapture requestCapture) And_we_have_a_service_with_request_capture_and_json_response()
    {
        var requestCapture = new RequestCapture();
        var movies = Given_we_have_a_list_of_movies();
        var json = JsonSerializer.Serialize(movies);
        
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => requestCapture.CapturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });

        var service = CreateMovieService(handler);
        
        return (service, requestCapture);
    }

    MovieService And_we_have_a_service_that_returns_ok()
    {
        var handler = CreateMockHttpHandler(HttpStatusCode.OK);
        return CreateMovieService(handler);
    }

    MovieService And_we_have_a_service_that_returns_no_content()
    {
        var handler = CreateMockHttpHandler(HttpStatusCode.NoContent);
        return CreateMovieService(handler);
    }

    MovieService And_we_have_a_service_that_returns_created()
    {
        var handler = CreateMockHttpHandler(HttpStatusCode.Created);
        return CreateMovieService(handler);
    }

    MovieService And_we_have_a_service_that_returns_movies(List<MovieDto> movies)
    {
        var json = JsonSerializer.Serialize(movies);
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });
        
        return CreateMovieService(handler);
    }

    #endregion

    #region When Methods

    async Task<HttpResponseMessage> When_we_delete_the_movie(
        MovieService service,
        Guid movieId)
    {
        return await service.DeleteAsync(movieId);
    }

    async Task<HttpResponseMessage> When_we_update_the_movie(
        MovieService service,
        Guid movieId,
        MovieDto movieDto)
    {
        return await service.UpdateAsync(movieId, movieDto);
    }

    async Task<HttpResponseMessage> When_we_create_the_movie(
        MovieService service,
        MovieDto movieDto)
    {
        return await service.CreateAsync(movieDto);
    }

    async Task<List<MovieDto>?> When_we_get_all_movies(MovieService service)
    {
        return await service.GetAllAsync();
    }

    #endregion

    #region Then Methods

    async Task Then_unauthorized_exception_should_be_thrown(
        MovieService service,
        Guid movieId)
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.DeleteAsync(movieId));
    }

    async Task Then_unauthorized_exception_should_be_thrown_on_update(
        MovieService service,
        Guid movieId,
        MovieDto movieDto)
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.UpdateAsync(movieId, movieDto));
    }

    async Task Then_unauthorized_exception_should_be_thrown_on_create(
        MovieService service,
        MovieDto movieDto)
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateAsync(movieDto));
    }

    void Then_correct_delete_endpoint_should_be_called(
        RequestCapture requestCapture,
        Guid movieId)
    {
        requestCapture.CapturedRequest.Should().NotBeNull();
        requestCapture.CapturedRequest!.Method.Should().Be(HttpMethod.Delete);
        requestCapture.CapturedRequest.RequestUri!.AbsolutePath.Should().Be($"/api/movies/{movieId}");
    }

    void Then_correct_update_endpoint_should_be_called(
        RequestCapture requestCapture,
        Guid movieId)
    {
        requestCapture.CapturedRequest.Should().NotBeNull();
        requestCapture.CapturedRequest!.Method.Should().Be(HttpMethod.Put);
        requestCapture.CapturedRequest.RequestUri!.AbsolutePath.Should().Be("/api/movies");
        requestCapture.CapturedRequest.RequestUri!.Query.Should().Be($"?id={movieId}");
    }

    void Then_correct_create_endpoint_should_be_called(RequestCapture requestCapture)
    {
        requestCapture.CapturedRequest.Should().NotBeNull();
        requestCapture.CapturedRequest!.Method.Should().Be(HttpMethod.Post);
        requestCapture.CapturedRequest.RequestUri!.AbsolutePath.Should().Be("/api/movies");
    }

    void Then_correct_getall_endpoint_should_be_called(RequestCapture requestCapture)
    {
        requestCapture.CapturedRequest.Should().NotBeNull();
        requestCapture.CapturedRequest!.Method.Should().Be(HttpMethod.Get);
        requestCapture.CapturedRequest.RequestUri!.AbsolutePath.Should().Be("/api/movies");
    }

    void Then_response_should_be_successful(HttpResponseMessage response)
    {
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    void Then_response_should_be_no_content(HttpResponseMessage response)
    {
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    void Then_response_should_be_created(HttpResponseMessage response)
    {
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    void Then_movies_should_be_returned(List<MovieDto>? movies, List<MovieDto> expectedMovies)
    {
        movies.Should().NotBeNull();
        movies.Should().HaveCount(expectedMovies.Count);
        movies![0].Title.Should().Be(expectedMovies[0].Title);
        movies[1].Title.Should().Be(expectedMovies[1].Title);
    }

    #endregion

    #region Helper Methods

    private Mock<HttpMessageHandler> CreateMockHttpHandler(HttpStatusCode statusCode)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode));
        
        return handler;
    }

    private MovieService CreateMovieService(Mock<HttpMessageHandler> handler)
    {
        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("AuthorizedClient")).Returns(httpClient);

        return new MovieService(factory.Object);
    }

    private class RequestCapture
    {
        public HttpRequestMessage? CapturedRequest { get; set; }
    }

    #endregion
}
