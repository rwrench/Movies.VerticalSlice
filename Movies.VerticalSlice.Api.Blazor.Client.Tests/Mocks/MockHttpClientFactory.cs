namespace Movies.VerticalSlice.Api.Blazor.Client.Tests.Mocks;

public class MockHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;
    private readonly Uri _base;
    public MockHttpClientFactory(HttpMessageHandler handler, Uri baseAddress)
    {
        _handler = handler;
        _base = baseAddress;
    }

    public HttpClient CreateClient(string name)
    {
        var client = new HttpClient(_handler) { BaseAddress = _base };
        return client;
    }
}
