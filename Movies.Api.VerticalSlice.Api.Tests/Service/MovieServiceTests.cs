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
        public async Task DeleteAsync_ReturnsBadRequest_ThrowsException()
        {
            var handler = new Mock<HttpMessageHandler>();
            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var httpClient = new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var service = new MovieService(factory.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(Guid.NewGuid()));
        }
    }
}
