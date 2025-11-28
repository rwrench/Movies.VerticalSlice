

namespace Movies.VerticalSlice.Api.Blazor.Client.Tests.TestHelpers
{
    // Capturing handler and factory
    public class CapturingHttpHandler : HttpMessageHandler
    {
        private Func<HttpRequestMessage, Task<HttpResponseMessage>> _responder;
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        public CapturingHttpHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) => _responder = responder;

        public void SetResponder(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) => _responder = responder;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content is not null)
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            return await _responder(request);
        }
    }
}



