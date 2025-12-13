using FluentAssertions;
using Moq;
using Moq.Protected;
using Movies.VerticalSlice.Api.Services;
using System.Net;
using Xunit;
    
namespace Movies.Api.VerticalSlice.Api.Tests.Service;

public class MovieServiceTests
{
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
        Then_correct_endpoint_should_be_called(requestCapture, movieId);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsOK_CompletesSuccessfully()
    {
        var movieId = Given_we_have_a_movie_id();
        var service = And_we_have_a_service_that_returns_ok();
        var response = await When_we_delete_the_movie(service, movieId);
        Then_response_should_be_successful(response);
    }

    Guid Given_we_have_a_movie_id()
    {
        return Guid.NewGuid();
    }

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

    MovieService And_we_have_a_service_that_returns_ok()
    {
        var handler = CreateMockHttpHandler(HttpStatusCode.OK);
        return CreateMovieService(handler);
    }

    async Task<HttpResponseMessage> When_we_delete_the_movie(
        MovieService service,
        Guid movieId)
    {
        return await service.DeleteAsync(movieId);
    }

    async Task Then_unauthorized_exception_should_be_thrown(
        MovieService service,
        Guid movieId)
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.DeleteAsync(movieId));
    }

    void Then_correct_endpoint_should_be_called(
        RequestCapture requestCapture,
        Guid movieId)
    {
        requestCapture.CapturedRequest.Should().NotBeNull();
        requestCapture.CapturedRequest!.Method.Should().Be(HttpMethod.Delete);
        requestCapture.CapturedRequest.RequestUri!.AbsolutePath.Should().Be($"/api/movies/{movieId}");
    }

    void Then_response_should_be_successful(HttpResponseMessage response)
    {
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // Helper methods to reduce duplication
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

    // Helper class to capture the request by reference
    private class RequestCapture
    {
        public HttpRequestMessage? CapturedRequest { get; set; }
    }
}
