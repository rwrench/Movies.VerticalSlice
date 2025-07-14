using Microsoft.AspNetCore.Mvc.Testing;
using Movies.VerticalSlice.Api;

namespace Movies.Api.VerticalSlice.Api.Tests.Integration;
public class SmokeTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SmokeTest(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Api_Starts_And_Responds()
    {
        var response = await _client.GetAsync("/");
        Assert.NotNull(response); // Even a 404 is OK here, just proves the API starts
    }
}