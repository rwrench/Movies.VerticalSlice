using Moq;
using Moq.Protected;
using Movies.VerticalSlice.Api.Services;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Movies.Api.VerticalSlice.Api.Tests.Service
{

    public class MovieServiceTests
    {
        [Fact]
        public async Task DeleteAsync_ReturnsUnauthorized_ThrowsException()
        {
            var handler = new Mock<HttpMessageHandler>();
            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized));

            var httpClient = new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var service = new MovieService(factory.Object);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(Guid.NewGuid()));
        }
        [Fact]
        public async Task DeleteAsync_Returns_OK_WhenAuthHeaderIsSet()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var handler = new Mock<HttpMessageHandler>();
            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var httpClient = new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var service = new MovieService(factory.Object);
            

            // Act
            await service.DeleteAsync(Guid.NewGuid());

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.True(capturedRequest.Headers.Authorization != null, "Authorization header should be set.");
            Assert.Equal("Bearer", capturedRequest.Headers.Authorization!.Scheme);
            Assert.Equal("test-token", capturedRequest.Headers.Authorization!.Parameter);
        }
    }
}
