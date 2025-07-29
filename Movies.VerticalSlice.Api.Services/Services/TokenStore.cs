namespace Movies.VerticalSlice.Api.Services;

public class TokenStore
{
    public string? Token { get; set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
}
