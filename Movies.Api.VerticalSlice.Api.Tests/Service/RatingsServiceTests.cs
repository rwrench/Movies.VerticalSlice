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

public class RatingsServiceTests
{
    #region GetAll Tests

    [Fact]
    public async Task GetAllAsync_ReturnsUnauthorized_ThrowsException()
    {
        var service = And_we_have_a_service_that_returns_unauthorized();
        await Then_unauthorized_exception_should_be_thrown_on_getall(service);
    }

    [Fact]
    public async Task GetAllAsync_CallsCorrectEndpoint()
    {
        var (service, requestCapture) = And_we_have_a_service_with_request_capture_and_json_response();
        await When_we_get_all_ratings(service);
        Then_correct_getall_endpoint_should_be_called(requestCapture);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsRatings_Successfully()
    {
        var expectedRatings = Given_we_have_a_list_of_ratings();
        var service = And_we_have_a_service_that_returns_ratings(expectedRatings);
        var ratings = await When_we_get_all_ratings(service);
        Then_ratings_should_be_returned(ratings, expectedRatings);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task CreateAsync_WithValidRating_CallsCorrectEndpoint()
    {
        var ratingDto = Given_we_have_a_rating_dto();
        var (service, requestCapture) = And_we_have_a_service_with_request_capture_created();
        await When_we_create_the_rating(service, ratingDto);
        Then_correct_create_endpoint_should_be_called(requestCapture);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreated_CompletesSuccessfully()
    {
        var ratingDto = Given_we_have_a_rating_dto();
        var service = And_we_have_a_service_that_returns_created();
        var response = await When_we_create_the_rating(service, ratingDto);
        Then_response_should_be_created(response);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_WithValidRating_CallsCorrectEndpoint()
    {
        var ratingDto = Given_we_have_a_rating_dto();
        var (service, requestCapture) = And_we_have_a_service_with_request_capture();
        await When_we_update_the_rating(service, ratingDto);
        Then_correct_update_endpoint_should_be_called(requestCapture, ratingDto.Id);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNoContent_CompletesSuccessfully()
    {
        var ratingDto = Given_we_have_a_rating_dto();
        var service = And_we_have_a_service_that_returns_no_content();
        var response = await When_we_update_the_rating(service, ratingDto);
        Then_response_should_be_no_content(response);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_CallsCorrectEndpoint()
    {
        var ratingId = Given_we_have_a_rating_id();
        var (service, requestCapture) = And_we_have_a_service_with_request_capture();
        await When_we_delete_the_rating(service, ratingId);
        Then_correct_delete_endpoint_should_be_called(requestCapture, ratingId);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsOK_CompletesSuccessfully()
    {
        var ratingId = Given_we_have_a_rating_id();
        var service = And_we_have_a_service_that_returns_ok();
        var response = await When_we_delete_the_rating(service, ratingId);
        Then_response_should_be_successful(response);
    }

    #endregion

    #region GetAllMovieNames Tests

    [Fact]
    public async Task GetAllMovieNamesAsync_WithoutTitle_CallsCorrectEndpoint()
    {
        var (service, requestCapture) = And_we_have_a_service_with_request_capture_and_movie_names_response();
        await When_we_get_all_movie_names(service, "");
        Then_correct_getall_movie_names_endpoint_should_be_called_without_title(requestCapture);
    }

    [Fact]
    public async Task GetAllMovieNamesAsync_WithTitle_CallsCorrectEndpoint()
    {
        var title = "Test";
        var (service, requestCapture) = And_we_have_a_service_with_request_capture_and_movie_names_response();
        await When_we_get_all_movie_names(service, title);
        Then_correct_getall_movie_names_endpoint_should_be_called_with_title(requestCapture, title);
    }

    [Fact]
    public async Task GetAllMovieNamesAsync_ReturnsMovieNames_Successfully()
    {
        var expectedMovieNames = Given_we_have_a_list_of_movie_names();
        var service = And_we_have_a_service_that_returns_movie_names(expectedMovieNames);
        var movieNames = await When_we_get_all_movie_names(service, "");
        Then_movie_names_should_be_returned(movieNames, expectedMovieNames);
    }

    #endregion

    #region Given Methods

    Guid Given_we_have_a_rating_id()
    {
        return Guid.NewGuid();
    }

    MovieRatingWithNameDto Given_we_have_a_rating_dto()
    {
        return new MovieRatingWithNameDto
        {
            Id = Guid.NewGuid(),
            MovieId = Guid.NewGuid(),
            MovieName = "Test Movie",
            Rating = 4.5f,
            DateUpdated = DateTime.UtcNow
        };
    }

    List<MovieRatingWithNameDto> Given_we_have_a_list_of_ratings()
    {
        return new List<MovieRatingWithNameDto>
        {
            new MovieRatingWithNameDto { Id = Guid.NewGuid(), MovieId = Guid.NewGuid(), MovieName = "Movie 1", Rating = 4.5f, DateUpdated = DateTime.UtcNow },
            new MovieRatingWithNameDto { Id = Guid.NewGuid(), MovieId = Guid.NewGuid(), MovieName = "Movie 2", Rating = 3.5f, DateUpdated = DateTime.UtcNow }
        };
    }

    List<MovieNameDto> Given_we_have_a_list_of_movie_names()
    {
        return new List<MovieNameDto>
        {
            new MovieNameDto { MovieId = Guid.NewGuid(), MovieName = "Movie 1" },
            new MovieNameDto { MovieId = Guid.NewGuid(), MovieName = "Movie 2" }
        };
    }

    #endregion

    #region And Methods

    RatingsService And_we_have_a_service_that_returns_unauthorized()
    {
        var handler = CreateMockHttpHandler(HttpStatusCode.Unauthorized);
        return CreateRatingsService(handler);
    }

    (RatingsService service, RequestCapture requestCapture) And_we_have_a_service_with_request_capture()
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

        var service = CreateRatingsService(handler);
        
        return (service, requestCapture);
    }

    (RatingsService service, RequestCapture requestCapture) And_we_have_a_service_with_request_capture_created()
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

        var service = CreateRatingsService(handler);
        
        return (service, requestCapture);
    }

    (RatingsService service, RequestCapture requestCapture) And_we_have_a_service_with_request_capture_and_json_response()
    {
        var requestCapture = new RequestCapture();
        var ratings = Given_we_have_a_list_of_ratings();
        var json = JsonSerializer.Serialize(ratings);
        
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

        var service = CreateRatingsService(handler);
        
        return (service, requestCapture);
    }

    (RatingsService service, RequestCapture requestCapture) And_we_have_a_service_with_request_capture_and_movie_names_response()
    {
        var requestCapture = new RequestCapture();
        var movieNames = Given_we_have_a_list_of_movie_names();
        var json = JsonSerializer.Serialize(movieNames);
        
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

        var service = CreateRatingsService(handler);
        
        return (service, requestCapture);
    }

    RatingsService And_we_have_a_service_that_returns_ok()
    {
        var handler = CreateMockHttpHandler(HttpStatusCode.OK);
        return CreateRatingsService(handler);
    }

    RatingsService And_we_have_a_service_that_returns_no_content()
    {
        var handler = CreateMockHttpHandler(HttpStatusCode.NoContent);
        return CreateRatingsService(handler);
    }

    RatingsService And_we_have_a_service_that_returns_created()
    {
        var handler = CreateMockHttpHandler(HttpStatusCode.Created);
        return CreateRatingsService(handler);
    }

    RatingsService And_we_have_a_service_that_returns_ratings(List<MovieRatingWithNameDto> ratings)
    {
        var json = JsonSerializer.Serialize(ratings);
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
        
        return CreateRatingsService(handler);
    }

    RatingsService And_we_have_a_service_that_returns_movie_names(List<MovieNameDto> movieNames)
    {
        var json = JsonSerializer.Serialize(movieNames);
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
        
        return CreateRatingsService(handler);
    }

    #endregion

    #region When Methods

    async Task<List<MovieRatingWithNameDto>?> When_we_get_all_ratings(RatingsService service)
    {
        return await service.GetAllAsync();
    }

    async Task<HttpResponseMessage> When_we_create_the_rating(
        RatingsService service,
        MovieRatingWithNameDto ratingDto)
    {
        return await service.CreateAsync(ratingDto);
    }

    async Task<HttpResponseMessage> When_we_update_the_rating(
        RatingsService service,
        MovieRatingWithNameDto ratingDto)
    {
        return await service.UpdateAsync(ratingDto);
    }

    async Task<HttpResponseMessage> When_we_delete_the_rating(
        RatingsService service,
        Guid ratingId)
    {
        return await service.DeleteAsync(ratingId);
    }

    async Task<List<MovieNameDto>?> When_we_get_all_movie_names(
        RatingsService service,
        string title)
    {
        return await service.GetAllMovieNamesAsync(title);
    }

    #endregion

    #region Then Methods

    async Task Then_unauthorized_exception_should_be_thrown_on_getall(RatingsService service)
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.GetAllAsync());
    }

    void Then_correct_getall_endpoint_should_be_called(RequestCapture requestCapture)
    {
        requestCapture.CapturedRequest.Should().NotBeNull();
        requestCapture.CapturedRequest!.Method.Should().Be(HttpMethod.Get);
        requestCapture.CapturedRequest.RequestUri!.AbsolutePath.Should().Be(ApiEndpoints.Ratings.GetAll);
    }

    void Then_correct_create_endpoint_should_be_called(RequestCapture requestCapture)
    {
        requestCapture.CapturedRequest.Should().NotBeNull();
        requestCapture.CapturedRequest!.Method.Should().Be(HttpMethod.Post);
        requestCapture.CapturedRequest.RequestUri!.AbsolutePath.Should().Be(ApiEndpoints.Ratings.Base);
    }

    void Then_correct_update_endpoint_should_be_called(
        RequestCapture requestCapture,
        Guid ratingId)
    {
        requestCapture.CapturedRequest.Should().NotBeNull();
        requestCapture.CapturedRequest!.Method.Should().Be(HttpMethod.Put);
        requestCapture.CapturedRequest.RequestUri!.AbsolutePath.Should().Be($"{ApiEndpoints.Ratings.Base}/{ratingId}");
    }

    void Then_correct_delete_endpoint_should_be_called(
        RequestCapture requestCapture,
        Guid ratingId)
    {
        requestCapture.CapturedRequest.Should().NotBeNull();
        requestCapture.CapturedRequest!.Method.Should().Be(HttpMethod.Delete);
        requestCapture.CapturedRequest.RequestUri!.AbsolutePath.Should().Be($"{ApiEndpoints.Ratings.Base}/{ratingId}");
    }

    void Then_correct_getall_movie_names_endpoint_should_be_called_without_title(RequestCapture requestCapture)
    {
        requestCapture.CapturedRequest.Should().NotBeNull();
        requestCapture.CapturedRequest!.Method.Should().Be(HttpMethod.Get);
        requestCapture.CapturedRequest.RequestUri!.AbsolutePath.Should().Be($"{ApiEndpoints.Movies.Names}/");
    }

    void Then_correct_getall_movie_names_endpoint_should_be_called_with_title(
        RequestCapture requestCapture,
        string title)
    {
        requestCapture.CapturedRequest.Should().NotBeNull();
        requestCapture.CapturedRequest!.Method.Should().Be(HttpMethod.Get);
        requestCapture.CapturedRequest.RequestUri!.AbsolutePath.Should().Be(ApiEndpoints.Movies.Names);
        requestCapture.CapturedRequest.RequestUri!.Query.Should().Contain($"title={Uri.EscapeDataString(title)}");
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

    void Then_ratings_should_be_returned(
        List<MovieRatingWithNameDto>? ratings,
        List<MovieRatingWithNameDto> expectedRatings)
    {
        ratings.Should().NotBeNull();
        ratings.Should().HaveCount(expectedRatings.Count);
        ratings![0].MovieName.Should().Be(expectedRatings[0].MovieName);
        ratings[1].MovieName.Should().Be(expectedRatings[1].MovieName);
    }

    void Then_movie_names_should_be_returned(
        List<MovieNameDto>? movieNames,
        List<MovieNameDto> expectedMovieNames)
    {
        movieNames.Should().NotBeNull();
        movieNames.Should().HaveCount(expectedMovieNames.Count);
        movieNames![0].MovieName.Should().Be(expectedMovieNames[0].MovieName);
        movieNames[1].MovieName.Should().Be(expectedMovieNames[1].MovieName);
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

    private RatingsService CreateRatingsService(Mock<HttpMessageHandler> handler)
    {
        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("AuthorizedClient")).Returns(httpClient);

        return new RatingsService(factory.Object);
    }

    private class RequestCapture
    {
        public HttpRequestMessage? CapturedRequest { get; set; }
    }

    #endregion
}
