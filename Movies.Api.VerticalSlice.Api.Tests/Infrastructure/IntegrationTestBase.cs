using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Movies.VerticalSlice.Api.Configuration;
using Movies.VerticalSlice.Api.Data.Database;

namespace Movies.Api.VerticalSlice.Api.Tests.Infrastructure;

public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory<Program> Factory;
    private readonly IServiceScope _scope;
    protected readonly MoviesDbContext DbContext;
    protected const string UserId = "3fee2354-5f37-42cf-8799-b54340785433";
    protected const string UserName = "richardw";

    public IntegrationTestBase(CustomWebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        
        // Create a scope for database access in tests
        _scope = factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<MoviesDbContext>();
        DbContext.Database.EnsureCreated();
        ClearDatabase().GetAwaiter().GetResult();
    }

   

    /// <summary>
    /// Generate a valid JWT token for authenticated requests
    /// </summary>
    protected string GenerateJwtToken(string userId = "test-user-id", string userName = "testuser", string? email = null)
    {
        var configuration = Factory.Services.GetRequiredService<IConfiguration>();
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

        var keyBytes = Encoding.ASCII.GetBytes(jwtSettings!.Secret);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Email, email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes),
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Set authorization header with JWT token
    /// </summary>
    protected void SetAuthorizationHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Create authenticated client with JWT token
    /// </summary>
    protected HttpClient CreateAuthenticatedClient(string userId = "test-user-id", string userName = "testuser")
    {
        var client = Factory.CreateClient();
        var token = GenerateJwtToken(userId, userName);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Clear database for clean test state
    /// </summary>
    protected async Task ClearDatabase()
    {
        DbContext.Movies.RemoveRange(DbContext.Movies);
        DbContext.Ratings.RemoveRange(DbContext.Ratings);
        DbContext.Users.RemoveRange(DbContext.Users);
        DbContext.ApplicationLogs.RemoveRange(DbContext.ApplicationLogs);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Get count of entities in database
    /// </summary>
    protected async Task<int> GetMovieCount() => await Task.FromResult(DbContext.Movies.Count());
    protected async Task<int> GetRatingCount() => await Task.FromResult(DbContext.Ratings.Count());
    protected async Task<int> GetUserCount() => await Task.FromResult(DbContext.Users.Count());

    public void Dispose()
    {
        _scope?.Dispose();
        Client?.Dispose();
    }
}
